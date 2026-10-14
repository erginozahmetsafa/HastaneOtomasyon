using HastaneOtomasyon.Core.DTOs;

namespace HastaneOtomasyon.Core.Services;

public interface IHastaneService
{
	Task<IReadOnlyList<HastaneDto>> GetAllAsync(CancellationToken ct = default);
	Task<HastaneDetayDto> GetDetayAsync(int id, CancellationToken ct = default);

	// Sistem yöneticisi
	Task<HastaneDto> OlusturAsync(HastaneKaydetDto dto, CancellationToken ct = default);
	Task SilAsync(int id, CancellationToken ct = default);

	// Sistem yöneticisi veya o hastanenin yöneticisi
	Task GuncelleAsync(int id, HastaneKaydetDto dto, CancellationToken ct = default);
	Task<CalismaSaatiDto> CalismaSaatiEkleAsync(int hastaneId, ZamanAraligiDto dto, CancellationToken ct = default);
	Task CalismaSaatiSilAsync(int calismaSaatiId, CancellationToken ct = default);
}
