using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Exceptions;
using HastaneOtomasyon.Core.Repositories;
using HastaneOtomasyon.Core.Services;
using HastaneOtomasyon.Core.UnitOfWorks;
using HastaneOtomasyon.Service.Extensions;
using HastaneOtomasyon.Service.Mapping;
using HastaneOtomasyon.Service.Security;

namespace HastaneOtomasyon.Service.Services;

internal sealed class HesapService(
	IKisiRepository kisiRepository,
	KisiFabrikasi kisiFabrikasi,
	ISifreHasher sifreHasher,
	IUnitOfWork unitOfWork,
	IAktifKullanici aktifKullanici) : IHesapService
{
	// E-posta bulunamadığında da aynı maliyette doğrulama yapılır; yanıt süresinden
	// hangi e-postaların kayıtlı olduğu anlaşılamaz.
	private const string SahteHash = "100000.9woG2bs14W6VuFgcJ1F/6g==.sgOwZJEKOl1exa9aHRsci8B8AcLDiHeHOIEsslXrDKI=";

	public async Task<OturumBilgisi> HastaKayitAsync(HastaKayitDto dto, CancellationToken ct = default)
	{
		var hasta = await kisiFabrikasi.OlusturAsync<Hasta>(dto, ct);
		hasta.DogumTarihi = dto.DogumTarihi.Date;
		hasta.Cinsiyet = dto.Cinsiyet;
		hasta.KanGrubu = dto.KanGrubu;

		await kisiRepository.AddAsync(hasta, ct);
		await unitOfWork.SaveChangesAsync(ct);

		return hasta.ToOturumBilgisi();
	}

	public async Task<OturumBilgisi?> GirisAsync(GirisDto dto, CancellationToken ct = default)
	{
		var kisi = await kisiRepository.GetByEmailAsync(KisiFabrikasi.EmailNormalizeEt(dto.Email), ct);

		var sifreDogru = sifreHasher.Dogrula(dto.Sifre, kisi?.SifreHash ?? SahteHash);
		if (kisi is null || !sifreDogru)
			return null;

		// Bu bilgi yalnızca şifre doğruysa verilir.
		if (!kisi.AktifMi)
			throw new YetkisizIslemException("Hesabınız pasif durumdadır. Kurum yönetimiyle iletişime geçin.");

		return kisi.ToOturumBilgisi();
	}

	public async Task<OturumBilgisi> SifreDegistirAsync(SifreDegistirDto dto, CancellationToken ct = default)
	{
		var kisiId = aktifKullanici.GirisYapmisOlmali();
		var kisi = await kisiRepository.GetByIdOrThrowAsync(kisiId, ct);

		if (!sifreHasher.Dogrula(dto.MevcutSifre, kisi.SifreHash))
			throw new IsKuraliException("Mevcut şifre hatalı.");
		if (dto.YeniSifre == dto.MevcutSifre)
			throw new IsKuraliException("Yeni şifre mevcut şifreden farklı olmalıdır.");

		kisi.SifreyiDegistir(sifreHasher.Hashle(dto.YeniSifre));
		await unitOfWork.SaveChangesAsync(ct);

		return kisi.ToOturumBilgisi();
	}

	public Task<bool> OturumGecerliMiAsync(int kisiId, Guid guvenlikDamgasi, CancellationToken ct = default) =>
		kisiRepository.OturumGecerliMiAsync(kisiId, guvenlikDamgasi, ct);
}
