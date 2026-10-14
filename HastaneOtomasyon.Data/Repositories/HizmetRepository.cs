using System.Linq.Expressions;
using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HastaneOtomasyon.Data.Repositories;

public class HizmetRepository(AppDbContext context) : GenericRepository<Hizmet>(context), IHizmetRepository
{
	public async Task<IReadOnlyList<Hizmet>> GetByPoliklinikAsync(int poliklinikId, bool sadeceAktif = true, CancellationToken ct = default) =>
		await Listele(h => h.PoliklinikId == poliklinikId, sadeceAktif).ToListAsync(ct);

	public async Task<IReadOnlyList<Hizmet>> GetByHastaneAsync(int hastaneId, bool sadeceAktif = true, CancellationToken ct = default) =>
		await Listele(h => h.Poliklinik.HastaneId == hastaneId, sadeceAktif).ToListAsync(ct);

	private IQueryable<Hizmet> Listele(Expression<Func<Hizmet, bool>> filtre, bool sadeceAktif)
	{
		var sorgu = Set.AsNoTracking().Where(filtre);
		if (sadeceAktif)
			sorgu = sorgu.Where(h => h.AktifMi);

		return sorgu.OrderBy(h => h.Ad);
	}
}
