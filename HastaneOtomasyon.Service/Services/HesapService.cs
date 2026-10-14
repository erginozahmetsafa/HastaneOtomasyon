using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Repositories;
using HastaneOtomasyon.Core.Services;
using HastaneOtomasyon.Core.UnitOfWorks;
using HastaneOtomasyon.Service.Mapping;

namespace HastaneOtomasyon.Service.Services;

internal sealed class HesapService(
	IKisiRepository kisiRepository,
	KisiFabrikasi kisiFabrikasi,
	ISifreHasher sifreHasher,
	IUnitOfWork unitOfWork) : IHesapService
{
	public async Task<KullaniciDto> HastaKayitAsync(HastaKayitDto dto, CancellationToken ct = default)
	{
		var hasta = await kisiFabrikasi.OlusturAsync<Hasta>(dto, ct);
		hasta.DogumTarihi = dto.DogumTarihi.Date;
		hasta.Cinsiyet = dto.Cinsiyet;
		hasta.KanGrubu = dto.KanGrubu;

		await kisiRepository.AddAsync(hasta, ct);
		await unitOfWork.SaveChangesAsync(ct);

		return hasta.ToKullaniciDto();
	}

	public async Task<KullaniciDto?> GirisAsync(GirisDto dto, CancellationToken ct = default)
	{
		var kisi = await kisiRepository.GetByEmailAsync(KisiFabrikasi.EmailNormalizeEt(dto.Email), ct);

		return kisi is not null && sifreHasher.Dogrula(dto.Sifre, kisi.SifreHash)
			? kisi.ToKullaniciDto()
			: null;
	}
}
