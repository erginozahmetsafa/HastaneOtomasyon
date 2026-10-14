using HastaneOtomasyon.Core.Entities;

namespace HastaneOtomasyon.Core.Repositories;

public interface IHizmetRepository : IGenericRepository<Hizmet>
{
	Task<IReadOnlyList<Hizmet>> GetByPoliklinikAsync(int poliklinikId, bool sadeceAktif = true, CancellationToken ct = default);
	Task<IReadOnlyList<Hizmet>> GetByHastaneAsync(int hastaneId, bool sadeceAktif = true, CancellationToken ct = default);
}
