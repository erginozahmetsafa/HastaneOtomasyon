using System.Linq.Expressions;
using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Enums;
using HastaneOtomasyon.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HastaneOtomasyon.Data.Repositories;

public class RandevuRepository(AppDbContext context) : GenericRepository<Randevu>(context), IRandevuRepository
{
	public Task<Randevu?> GetDetayAsync(int id, CancellationToken ct = default) =>
		DetayliSorgu().FirstOrDefaultAsync(r => r.Id == id, ct);

	public Task<Randevu?> GetIslemIcinAsync(int id, CancellationToken ct = default) =>
		Set.Include(r => r.Doktor).FirstOrDefaultAsync(r => r.Id == id, ct);

	public async Task<IReadOnlyList<Randevu>> GetByDoktorAsync(int doktorId, DateTime baslangic, DateTime bitis, CancellationToken ct = default) =>
		await DetayliSorgu()
			.Where(r => r.DoktorId == doktorId && r.Baslangic >= baslangic && r.Baslangic < bitis)
			.OrderBy(r => r.Baslangic)
			.ToListAsync(ct);

	public async Task<IReadOnlyList<Randevu>> GetByHastaAsync(int hastaId, CancellationToken ct = default) =>
		await DetayliSorgu()
			.Where(r => r.HastaId == hastaId)
			.OrderByDescending(r => r.Baslangic)
			.ToListAsync(ct);

	public async Task<IReadOnlyList<Randevu>> GetOnayBekleyenlerAsync(int hastaneId, CancellationToken ct = default) =>
		await DetayliSorgu()
			.Where(r => r.Durum == RandevuDurum.Bekliyor && r.Doktor.HastaneId == hastaneId)
			.OrderBy(r => r.Baslangic)
			.ToListAsync(ct);

	public async Task<IReadOnlyList<Randevu>> GetGelecekAktifRandevularAsync(int doktorId, DateTime simdi, CancellationToken ct = default) =>
		await Set
			.Where(r =>
				r.DoktorId == doktorId &&
				r.Baslangic > simdi &&
				(r.Durum == RandevuDurum.Bekliyor || r.Durum == RandevuDurum.Onaylandi))
			.ToListAsync(ct);

	public Task<bool> DoktorCakismaVarMiAsync(int doktorId, DateTime baslangic, DateTime bitis, int? haricRandevuId = null, CancellationToken ct = default) =>
		CakismaVarMiAsync(r => r.DoktorId == doktorId, baslangic, bitis, haricRandevuId, ct);

	public Task<bool> HastaCakismaVarMiAsync(int hastaId, DateTime baslangic, DateTime bitis, int? haricRandevuId = null, CancellationToken ct = default) =>
		CakismaVarMiAsync(r => r.HastaId == hastaId, baslangic, bitis, haricRandevuId, ct);

	// Tüm okuma sorguları aynı ilişkileri yükler; böylece DTO dönüşümü her yerde güvenlidir.
	private IQueryable<Randevu> DetayliSorgu() =>
		Set.AsNoTracking()
			.Include(r => r.Hasta)
			.Include(r => r.Doktor).ThenInclude(d => d.Poliklinik)
			.Include(r => r.Hizmet);

	// İki aralık, biri diğeri bitmeden başlıyorsa çakışır: [a1, a2) ∩ [b1, b2) ≠ ∅ ⇔ a1 < b2 && b1 < a2
	private Task<bool> CakismaVarMiAsync(Expression<Func<Randevu, bool>> sahipFiltresi, DateTime baslangic, DateTime bitis, int? haricRandevuId, CancellationToken ct) =>
		Set.Where(sahipFiltresi)
			.AnyAsync(r =>
				(r.Durum == RandevuDurum.Bekliyor || r.Durum == RandevuDurum.Onaylandi) &&
				r.Baslangic < bitis &&
				baslangic < r.Bitis &&
				(haricRandevuId == null || r.Id != haricRandevuId), ct);
}
