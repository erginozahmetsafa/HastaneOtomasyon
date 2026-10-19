using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Enums;
using HastaneOtomasyon.Core.Exceptions;
using HastaneOtomasyon.Core.Repositories;
using HastaneOtomasyon.Core.Services;
using HastaneOtomasyon.Core.UnitOfWorks;
using HastaneOtomasyon.Service.Services;
using HastaneOtomasyon.UnitTests.Yardimcilar;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace HastaneOtomasyon.UnitTests.Services;

// Personel pasifleştirme: işten ayrılan çalışanın kaydı silinmez, girişi ve yeni randevuları engellenir.
public class PersonelServiceTests
{
	private const int HastaneId = TestVerisi.HastaneId;

	private readonly Mock<IKisiRepository> _kisiRepository = new();
	private readonly Mock<IGenericRepository<Calisan>> _calisanRepository = new();
	private readonly Mock<IRandevuRepository> _randevuRepository = new();
	private readonly Mock<IUnitOfWork> _unitOfWork = new();
	private readonly FakeTimeProvider _zaman = new(new DateTimeOffset(TestVerisi.Simdi, TimeSpan.Zero));

	private PersonelService Servis(IAktifKullanici kullanici) => new(
		_kisiRepository.Object,
		_calisanRepository.Object,
		Mock.Of<IDoktorRepository>(),
		Mock.Of<IHastaneRepository>(),
		Mock.Of<IGenericRepository<Poliklinik>>(),
		Mock.Of<IHizmetRepository>(),
		_randevuRepository.Object,
		new KisiFabrikasi(_kisiRepository.Object, Mock.Of<ISifreHasher>()),
		_unitOfWork.Object,
		kullanici,
		_zaman);

	private T CalisanVar<T>(T calisan) where T : Calisan
	{
		_calisanRepository
			.Setup(r => r.GetByIdAsync(calisan.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(calisan);
		return calisan;
	}

	[Fact]
	public async Task Pasiflestir_Doktor_GelecekRandevulariIptalEdilir()
	{
		var doktor = CalisanVar(TestVerisi.Doktor(10, HastaneId));
		List<Randevu> gelecekRandevular =
		[
			TestVerisi.Randevu(TestVerisi.Simdi.AddDays(1), RandevuDurum.Bekliyor, doktor: doktor, id: 1),
			TestVerisi.Randevu(TestVerisi.Simdi.AddDays(2), RandevuDurum.Onaylandi, doktor: doktor, id: 2)
		];
		_randevuRepository
			.Setup(r => r.GetGelecekAktifRandevularAsync(doktor.Id, TestVerisi.Simdi, It.IsAny<CancellationToken>()))
			.ReturnsAsync(gelecekRandevular);

		var sonuc = await Servis(TestKullanici.YoneticiOlarak(2, HastaneId)).PasiflestirAsync(doktor.Id);

		Assert.False(doktor.AktifMi);
		Assert.Equal(2, sonuc.IptalEdilenRandevuSayisi);
		Assert.All(gelecekRandevular, r =>
		{
			Assert.Equal(RandevuDurum.IptalEdildi, r.Durum);
			Assert.NotNull(r.IptalRedNedeni);
		});
		_unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Pasiflestir_SaglikPersoneli_RandevularaDokunulmaz()
	{
		var personel = CalisanVar(new SaglikPersoneli { Id = 20, HastaneId = HastaneId });

		var sonuc = await Servis(TestKullanici.YoneticiOlarak(2, HastaneId)).PasiflestirAsync(personel.Id);

		Assert.False(personel.AktifMi);
		Assert.Equal(0, sonuc.IptalEdilenRandevuSayisi);
		_randevuRepository.Verify(
			r => r.GetGelecekAktifRandevularAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task Pasiflestir_BaskaHastaneninPersoneli_HataVerir()
	{
		var doktor = CalisanVar(TestVerisi.Doktor(10, HastaneId));

		await Assert.ThrowsAsync<YetkisizIslemException>(() =>
			Servis(TestKullanici.YoneticiOlarak(2, TestVerisi.DigerHastaneId)).PasiflestirAsync(doktor.Id));

		Assert.True(doktor.AktifMi);
		_unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	// Hastane yöneticisi kendi hastanesindeki başka bir yöneticiyi pasifleştiremez.
	[Fact]
	public async Task Pasiflestir_YoneticiyiHastaneYoneticisi_HataVerir()
	{
		var yonetici = CalisanVar(new Yonetici { Id = 30, HastaneId = HastaneId });

		await Assert.ThrowsAsync<YetkisizIslemException>(() =>
			Servis(TestKullanici.YoneticiOlarak(2, HastaneId)).PasiflestirAsync(yonetici.Id));

		Assert.True(yonetici.AktifMi);
	}

	[Fact]
	public async Task Pasiflestir_YoneticiyiSistemYoneticisi_Pasiflestirebilir()
	{
		var yonetici = CalisanVar(new Yonetici { Id = 30, HastaneId = HastaneId });

		await Servis(TestKullanici.SistemYoneticisiOlarak()).PasiflestirAsync(yonetici.Id);

		Assert.False(yonetici.AktifMi);
	}

	[Fact]
	public async Task Pasiflestir_ZatenPasif_HataVerir()
	{
		var doktor = TestVerisi.Doktor(10, HastaneId);
		doktor.AktifMi = false;
		CalisanVar(doktor);

		await Assert.ThrowsAsync<IsKuraliException>(() =>
			Servis(TestKullanici.YoneticiOlarak(2, HastaneId)).PasiflestirAsync(doktor.Id));
	}

	[Fact]
	public async Task Pasiflestir_CalisanYok_BulunamadiHatasi()
	{
		await Assert.ThrowsAsync<BulunamadiException>(() =>
			Servis(TestKullanici.SistemYoneticisiOlarak()).PasiflestirAsync(999));
	}

	[Fact]
	public async Task Aktiflestir_PasifCalisan_TekrarAktifOlur()
	{
		var doktor = TestVerisi.Doktor(10, HastaneId);
		doktor.AktifMi = false;
		CalisanVar(doktor);

		await Servis(TestKullanici.YoneticiOlarak(2, HastaneId)).AktiflestirAsync(doktor.Id);

		Assert.True(doktor.AktifMi);
		_unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}
