using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HastaneOtomasyon.Data.Repositories;

public class DoktorRepository(AppDbContext context) : GenericRepository<Doktor>(context), IDoktorRepository
{
	public Task<Doktor?> GetDetayAsync(int id, CancellationToken ct = default) =>
		Set.AsNoTracking()
			.Include(d => d.Poliklinik)
			.Include(d => d.Musaitlikler)
			.Include(d => d.DoktorHizmetler).ThenInclude(dh => dh.Hizmet)
			.AsSplitQuery()
			.FirstOrDefaultAsync(d => d.Id == id, ct);

	public async Task<IReadOnlyList<Doktor>> GetByPoliklinikAsync(int poliklinikId, CancellationToken ct = default) =>
		await Set.AsNoTracking()
			.Where(d => d.PoliklinikId == poliklinikId)
			.OrderBy(d => d.Ad).ThenBy(d => d.Soyad)
			.ToListAsync(ct);

	public async Task<IReadOnlyList<Doktor>> GetByHizmetAsync(int hizmetId, CancellationToken ct = default) =>
		await Set.AsNoTracking()
			.Where(d => d.DoktorHizmetler.Any(dh => dh.HizmetId == hizmetId))
			.OrderBy(d => d.Ad).ThenBy(d => d.Soyad)
			.ToListAsync(ct);

	public Task<DoktorHizmet?> GetDoktorHizmetAsync(int doktorId, int hizmetId, CancellationToken ct = default) =>
		Context.DoktorHizmetler.AsNoTracking()
			.Include(dh => dh.Hizmet)
			.FirstOrDefaultAsync(dh => dh.DoktorId == doktorId && dh.HizmetId == hizmetId, ct);
}
