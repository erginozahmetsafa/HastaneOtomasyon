using System.Linq.Expressions;
using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Enums;
using HastaneOtomasyon.Core.Exceptions;
using HastaneOtomasyon.Core.Repositories;
using HastaneOtomasyon.Core.Services;
using HastaneOtomasyon.Core.UnitOfWorks;
using HastaneOtomasyon.Service.Security;
using HastaneOtomasyon.Service.Services;
using HastaneOtomasyon.UnitTests.Yardimcilar;
using Moq;

namespace HastaneOtomasyon.UnitTests.Services;

public class HesapServiceTests
{
	private const string DogruSifre = "Sifre123";

	private readonly Mock<IKisiRepository> _kisiRepository = new();
	private readonly Mock<ISifreHasher> _sifreHasher = new();
	private readonly Mock<IUnitOfWork> _unitOfWork = new();

	private readonly Hasta _hasta = TestVerisi.Hasta(5);

	public HesapServiceTests()
	{
		_hasta.SifreHash = "kayitli-hash";

		_sifreHasher.Setup(h => h.Hashle(It.IsAny<string>())).Returns((string s) => $"hash({s})");
		_sifreHasher.Setup(h => h.Dogrula(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
		_sifreHasher.Setup(h => h.Dogrula(DogruSifre, "kayitli-hash")).Returns(true);

		_kisiRepository
			.Setup(r => r.GetByEmailAsync("mehmet@ornek.com", It.IsAny<CancellationToken>()))
			.ReturnsAsync(_hasta);
		_kisiRepository
			.Setup(r => r.GetByIdAsync(_hasta.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(_hasta);
	}

	private HesapService Servis(IAktifKullanici? kullanici = null) => new(
		_kisiRepository.Object,
		new KisiFabrikasi(_kisiRepository.Object, _sifreHasher.Object),
		_sifreHasher.Object,
		_unitOfWork.Object,
		kullanici ?? TestKullanici.Anonim);

	private static GirisDto Giris(string email, string sifre) => new() { Email = email, Sifre = sifre };

	// ---- Giriş ----

	[Fact]
	public async Task Giris_DogruBilgiler_OturumBilgisiDoner()
	{
		var oturum = await Servis().GirisAsync(Giris("mehmet@ornek.com", DogruSifre));

		Assert.NotNull(oturum);
		Assert.Equal(_hasta.Id, oturum.Kullanici.Id);
		Assert.Equal(Roller.Hasta, oturum.Kullanici.Rol);
		Assert.Equal(_hasta.GuvenlikDamgasi, oturum.GuvenlikDamgasi);
	}

	[Fact]
	public async Task Giris_EpostaKirpilirVeKucukHarfeCevrilir()
	{
		var oturum = await Servis().GirisAsync(Giris("  Mehmet@ORNEK.com ", DogruSifre));

		Assert.NotNull(oturum);
	}

	[Fact]
	public async Task Giris_YanlisSifre_NullDoner()
	{
		Assert.Null(await Servis().GirisAsync(Giris("mehmet@ornek.com", "Yanlis123")));
	}

	// Kayıtsız e-postada da şifre doğrulaması yapılır; yanıt süresinden hangi e-postaların kayıtlı olduğu anlaşılamaz.
	[Fact]
	public async Task Giris_KayitsizEposta_YineDeSifreDogrulanirVeNullDoner()
	{
		string? kullanilanHash = null;
		_sifreHasher
			.Setup(h => h.Dogrula(DogruSifre, It.IsAny<string>()))
			.Callback((string _, string hash) => kullanilanHash = hash)
			.Returns(false);

		var oturum = await Servis().GirisAsync(Giris("yok@ornek.com", DogruSifre));

		Assert.Null(oturum);
		_sifreHasher.Verify(h => h.Dogrula(DogruSifre, It.IsAny<string>()), Times.Once);

		// Sahte hash gerçek hash'lerle aynı biçimde ve iterasyon sayısında olmalı; aksi hâlde
		// doğrulama erken biter ve yanıt süresi e-postanın kayıtsız olduğunu ele verir.
		var gercekHash = new SifreHasher().Hashle("herhangi");
		Assert.NotNull(kullanilanHash);
		Assert.Equal(gercekHash.Split('.')[0], kullanilanHash.Split('.')[0]);
		Assert.Equal(gercekHash.Length, kullanilanHash.Length);
	}

	[Fact]
	public async Task Giris_PasifHesapDogruSifre_HesapPasifHatasiVerir()
	{
		_hasta.AktifMi = false;

		var hata = await Assert.ThrowsAsync<YetkisizIslemException>(() => Servis().GirisAsync(Giris("mehmet@ornek.com", DogruSifre)));

		Assert.Contains("pasif", hata.Message);
	}

	// Şifreyi bilmeyen biri hesabın pasif olduğunu öğrenemez.
	[Fact]
	public async Task Giris_PasifHesapYanlisSifre_NullDoner()
	{
		_hasta.AktifMi = false;

		Assert.Null(await Servis().GirisAsync(Giris("mehmet@ornek.com", "Yanlis123")));
	}

	// ---- Kayıt ----

	[Fact]
	public async Task HastaKayit_YeniKullanici_HashlenmisSifreyleKaydedilir()
	{
		Kisi? eklenen = null;
		_kisiRepository
			.Setup(r => r.AddAsync(It.IsAny<Kisi>(), It.IsAny<CancellationToken>()))
			.Callback((Kisi k, CancellationToken _) => eklenen = k);

		var dto = new HastaKayitDto
		{
			TcKimlikNo = "12345678901",
			Ad = " Zeynep ",
			Soyad = " Kaya ",
			Email = " Zeynep@Ornek.com ",
			Sifre = "YeniSifre1",
			DogumTarihi = new DateTime(1995, 5, 5, 14, 30, 0)
		};

		var oturum = await Servis().HastaKayitAsync(dto);

		var hasta = Assert.IsType<Hasta>(eklenen);
		Assert.Equal("Zeynep", hasta.Ad);
		Assert.Equal("Kaya", hasta.Soyad);
		Assert.Equal("zeynep@ornek.com", hasta.Email);
		Assert.Equal("hash(YeniSifre1)", hasta.SifreHash);
		Assert.Equal(new DateTime(1995, 5, 5), hasta.DogumTarihi); // saat bilgisi atılır
		Assert.Equal(Roller.Hasta, oturum.Kullanici.Rol);
		_unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task HastaKayit_EpostaVeyaTcKayitli_HataVerir()
	{
		_kisiRepository
			.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Kisi, bool>>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		var dto = new HastaKayitDto { TcKimlikNo = "12345678901", Ad = "A", Soyad = "B", Email = "mehmet@ornek.com", Sifre = "Sifre123" };

		await Assert.ThrowsAsync<IsKuraliException>(() => Servis().HastaKayitAsync(dto));
		_kisiRepository.Verify(r => r.AddAsync(It.IsAny<Kisi>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	// ---- Şifre değiştirme ----

	[Fact]
	public async Task SifreDegistir_GirisYapilmamis_HataVerir()
	{
		await Assert.ThrowsAsync<YetkisizIslemException>(() =>
			Servis().SifreDegistirAsync(new SifreDegistirDto { MevcutSifre = DogruSifre, YeniSifre = "YeniSifre1" }));
	}

	[Fact]
	public async Task SifreDegistir_MevcutSifreHatali_HataVerir()
	{
		var hata = await Assert.ThrowsAsync<IsKuraliException>(() =>
			Servis(TestKullanici.HastaOlarak(_hasta.Id))
				.SifreDegistirAsync(new SifreDegistirDto { MevcutSifre = "Yanlis123", YeniSifre = "YeniSifre1" }));

		Assert.Contains("Mevcut şifre", hata.Message);
		Assert.Equal("kayitli-hash", _hasta.SifreHash);
	}

	[Fact]
	public async Task SifreDegistir_YeniSifreEskisiyleAyni_HataVerir()
	{
		await Assert.ThrowsAsync<IsKuraliException>(() =>
			Servis(TestKullanici.HastaOlarak(_hasta.Id))
				.SifreDegistirAsync(new SifreDegistirDto { MevcutSifre = DogruSifre, YeniSifre = DogruSifre }));
	}

	[Fact]
	public async Task SifreDegistir_Basarili_HashVeGuvenlikDamgasiYenilenir()
	{
		var eskiDamga = _hasta.GuvenlikDamgasi;

		var oturum = await Servis(TestKullanici.HastaOlarak(_hasta.Id))
			.SifreDegistirAsync(new SifreDegistirDto { MevcutSifre = DogruSifre, YeniSifre = "YeniSifre1" });

		Assert.Equal("hash(YeniSifre1)", _hasta.SifreHash);
		Assert.NotEqual(eskiDamga, _hasta.GuvenlikDamgasi);
		Assert.Equal(_hasta.GuvenlikDamgasi, oturum.GuvenlikDamgasi); // istemciye yeni token verilir
		_unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}
