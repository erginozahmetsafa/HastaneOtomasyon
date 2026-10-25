using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Exceptions;
using HastaneOtomasyon.Core.Repositories;
using HastaneOtomasyon.Core.Services;
using HastaneOtomasyon.Core.UnitOfWorks;
using HastaneOtomasyon.Service.Services;
using HastaneOtomasyon.UnitTests.Yardimcilar;
using Moq;

namespace HastaneOtomasyon.UnitTests.Services;

// Hizmet listesi herkese açıktır; kullanımdan kaldırılmış hizmetleri yalnızca hastanenin yönetimi görür.
public class HizmetServiceTests
{
	private const int HastaneId = TestVerisi.HastaneId;

	private readonly Mock<IHizmetRepository> _hizmetRepository = new();

	public HizmetServiceTests()
	{
		_hizmetRepository
			.Setup(r => r.GetByHastaneAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync([TestVerisi.Hizmet()]);
	}

	private HizmetService Servis(IAktifKullanici kullanici) => new(
		_hizmetRepository.Object,
		Mock.Of<IGenericRepository<Poliklinik>>(),
		Mock.Of<IUnitOfWork>(),
		kullanici);

	[Fact]
	public async Task GetByHastane_Anonim_YalnizcaAktifHizmetler()
	{
		var hizmetler = await Servis(TestKullanici.Anonim).GetByHastaneAsync(HastaneId);

		Assert.Single(hizmetler);
		_hizmetRepository.Verify(r => r.GetByHastaneAsync(HastaneId, true, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task GetByHastane_PasiflerDahil_HastaneYoneticisi_PasiflerDeGelir()
	{
		await Servis(TestKullanici.YoneticiOlarak(2, HastaneId)).GetByHastaneAsync(HastaneId, pasiflerDahil: true);

		_hizmetRepository.Verify(r => r.GetByHastaneAsync(HastaneId, false, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task GetByHastane_PasiflerDahil_BaskaHastaneninYoneticisi_YetkisizIslem()
	{
		await Assert.ThrowsAsync<YetkisizIslemException>(() =>
			Servis(TestKullanici.YoneticiOlarak(2, TestVerisi.DigerHastaneId)).GetByHastaneAsync(HastaneId, pasiflerDahil: true));
	}

	[Fact]
	public async Task GetByHastane_PasiflerDahil_Anonim_YetkisizIslem()
	{
		await Assert.ThrowsAsync<YetkisizIslemException>(() =>
			Servis(TestKullanici.Anonim).GetByHastaneAsync(HastaneId, pasiflerDahil: true));
	}
}
