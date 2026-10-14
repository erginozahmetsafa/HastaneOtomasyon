using HastaneOtomasyon.Core.DTOs;

namespace HastaneOtomasyon.Core.Services;

public interface IMusaitlikService
{
	Task<IReadOnlyList<MusaitlikDto>> GetByDoktorAsync(int doktorId, CancellationToken ct = default);

	// Doktorun kendisi veya hastanesinin yöneticisi.
	// Müsaitlik, hastanenin çalışma saatleri içinde olmalı ve diğer dilimlerle çakışmamalıdır.
	Task<MusaitlikDto> EkleAsync(int doktorId, ZamanAraligiDto dto, CancellationToken ct = default);
	Task SilAsync(int musaitlikId, CancellationToken ct = default);
}
