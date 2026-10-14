using HastaneOtomasyon.Core.Entities;

namespace HastaneOtomasyon.Core.Repositories;

public interface IDoktorRepository : IGenericRepository<Doktor>
{
	// Poliklinik, müsaitlikler ve sunduğu hizmetlerle birlikte getirir.
	Task<Doktor?> GetDetayAsync(int id, CancellationToken ct = default);
	Task<IReadOnlyList<Doktor>> GetByPoliklinikAsync(int poliklinikId, CancellationToken ct = default);

	// Randevu alırken "işlem seç → uygun doktoru seç" akışı için.
	Task<IReadOnlyList<Doktor>> GetByHizmetAsync(int hizmetId, CancellationToken ct = default);

	// Doktor bu hizmeti sunmuyorsa null döner; Hizmet bilgisi dahil gelir (süre/ücret hesabı için).
	Task<DoktorHizmet?> GetDoktorHizmetAsync(int doktorId, int hizmetId, CancellationToken ct = default);
}
