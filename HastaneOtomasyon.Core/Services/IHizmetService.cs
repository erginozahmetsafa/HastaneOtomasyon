using HastaneOtomasyon.Core.DTOs;

namespace HastaneOtomasyon.Core.Services;

public interface IHizmetService
{
	Task<IReadOnlyList<HizmetDto>> GetByPoliklinikAsync(int poliklinikId, CancellationToken ct = default);
	Task<IReadOnlyList<HizmetDto>> GetByHastaneAsync(int hastaneId, CancellationToken ct = default);

	// Sistem yöneticisi veya o hastanenin yöneticisi.
	// Hizmetler silinmez (randevu geçmişi); AktifMi = false ile kullanımdan kaldırılır.
	Task<HizmetDto> OlusturAsync(HizmetOlusturDto dto, CancellationToken ct = default);
	Task GuncelleAsync(int id, HizmetGuncelleDto dto, CancellationToken ct = default);
}
