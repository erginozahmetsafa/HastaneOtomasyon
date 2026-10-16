using HastaneOtomasyon.Core.Entities;

namespace HastaneOtomasyon.Core.Repositories;

public interface IKisiRepository : IGenericRepository<Kisi>
{
	// Giriş işlemi için. Karşılaştırma veritabanı collation'ına bağlıdır (SQL Server varsayılanı: harf duyarsız).
	Task<Kisi?> GetByEmailAsync(string email, CancellationToken ct = default);

	// Her yetkili istekte çağrılır; tek bir EXISTS sorgusudur.
	Task<bool> OturumGecerliMiAsync(int id, Guid guvenlikDamgasi, CancellationToken ct = default);
}
