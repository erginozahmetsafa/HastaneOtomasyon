using HastaneOtomasyon.Core.DTOs;

namespace HastaneOtomasyon.Core.Services;

public interface IPoliklinikService
{
	Task<IReadOnlyList<PoliklinikDto>> GetByHastaneAsync(int hastaneId, CancellationToken ct = default);

	// Sistem yöneticisi veya o hastanenin yöneticisi
	Task<PoliklinikDto> OlusturAsync(PoliklinikOlusturDto dto, CancellationToken ct = default);
	Task GuncelleAsync(int id, PoliklinikGuncelleDto dto, CancellationToken ct = default);
	Task SilAsync(int id, CancellationToken ct = default);
}
