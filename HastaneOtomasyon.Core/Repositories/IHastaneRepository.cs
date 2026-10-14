using HastaneOtomasyon.Core.Entities;

namespace HastaneOtomasyon.Core.Repositories;

public interface IHastaneRepository : IGenericRepository<Hastane>
{
	// Poliklinikler ve çalışma saatleriyle birlikte getirir.
	Task<Hastane?> GetDetayAsync(int id, CancellationToken ct = default);
}
