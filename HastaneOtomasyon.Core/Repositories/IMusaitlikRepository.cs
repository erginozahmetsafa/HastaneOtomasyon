using HastaneOtomasyon.Core.Entities;

namespace HastaneOtomasyon.Core.Repositories;

public interface IMusaitlikRepository : IGenericRepository<Musaitlik>
{
	Task<IReadOnlyList<Musaitlik>> GetByDoktorAsync(int doktorId, CancellationToken ct = default);

	// İstenen aralık doktorun aktif bir müsaitlik dilimi içinde tamamen kalıyor mu?
	Task<bool> MusaitMiAsync(int doktorId, DayOfWeek gun, TimeSpan baslangic, TimeSpan bitis, CancellationToken ct = default);
}
