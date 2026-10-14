using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HastaneOtomasyon.Data.Repositories;

public class MusaitlikRepository(AppDbContext context) : GenericRepository<Musaitlik>(context), IMusaitlikRepository
{
	public async Task<IReadOnlyList<Musaitlik>> GetByDoktorAsync(int doktorId, CancellationToken ct = default) =>
		await Set.AsNoTracking()
			.Where(m => m.DoktorId == doktorId)
			.OrderBy(m => m.Gun).ThenBy(m => m.Baslangic)
			.ToListAsync(ct);

	public Task<bool> MusaitMiAsync(int doktorId, DayOfWeek gun, TimeSpan baslangic, TimeSpan bitis, CancellationToken ct = default) =>
		Set.AnyAsync(m =>
			m.DoktorId == doktorId &&
			m.AktifMi &&
			m.Gun == gun &&
			m.Baslangic <= baslangic &&
			m.Bitis >= bitis, ct);
}
