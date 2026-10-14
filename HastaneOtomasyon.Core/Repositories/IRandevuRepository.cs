using HastaneOtomasyon.Core.Entities;

namespace HastaneOtomasyon.Core.Repositories;

public interface IRandevuRepository : IGenericRepository<Randevu>
{
	// Hasta, doktor (poliklinikle) ve hizmet bilgisiyle birlikte getirir.
	Task<Randevu?> GetDetayAsync(int id, CancellationToken ct = default);

	Task<IReadOnlyList<Randevu>> GetByDoktorAsync(int doktorId, DateTime baslangic, DateTime bitis, CancellationToken ct = default);
	Task<IReadOnlyList<Randevu>> GetByHastaAsync(int hastaId, CancellationToken ct = default);
	Task<IReadOnlyList<Randevu>> GetOnayBekleyenlerAsync(int hastaneId, CancellationToken ct = default);

	// Yalnızca aktif (bekleyen/onaylı) randevular dikkate alınır.
	// Güncelleme senaryosunda randevunun kendisi haricRandevuId ile dışarıda bırakılır.
	Task<bool> DoktorCakismaVarMiAsync(int doktorId, DateTime baslangic, DateTime bitis, int? haricRandevuId = null, CancellationToken ct = default);
	Task<bool> HastaCakismaVarMiAsync(int hastaId, DateTime baslangic, DateTime bitis, int? haricRandevuId = null, CancellationToken ct = default);
}
