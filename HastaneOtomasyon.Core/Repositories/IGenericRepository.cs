using System.Linq.Expressions;
using HastaneOtomasyon.Core.Entities;

namespace HastaneOtomasyon.Core.Repositories;

// Tüm varlıklar için ortak veri erişim sözleşmesi.
// Listeleme metotları salt okunurdur (takip edilmez); güncellenecek kayıt GetByIdAsync ile alınmalıdır.
public interface IGenericRepository<T> where T : BaseEntity
{
	Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
	Task<IReadOnlyList<T>> GetAllAsync(Expression<Func<T, bool>>? filtre = null, CancellationToken ct = default);
	Task<bool> AnyAsync(Expression<Func<T, bool>> filtre, CancellationToken ct = default);

	Task AddAsync(T entity, CancellationToken ct = default);
	Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
	void Update(T entity);
	void Remove(T entity);
}
