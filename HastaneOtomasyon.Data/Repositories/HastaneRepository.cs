using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HastaneOtomasyon.Data.Repositories;

public class HastaneRepository(AppDbContext context) : GenericRepository<Hastane>(context), IHastaneRepository
{
	public Task<Hastane?> GetDetayAsync(int id, CancellationToken ct = default) =>
		Set.AsNoTracking()
			.Include(h => h.Poliklinikler)
			.Include(h => h.CalismaSaatleri)
			.AsSplitQuery()
			.FirstOrDefaultAsync(h => h.Id == id, ct);
}
