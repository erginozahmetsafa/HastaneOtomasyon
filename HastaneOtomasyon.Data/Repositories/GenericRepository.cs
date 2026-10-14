using System.Linq.Expressions;
using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HastaneOtomasyon.Data.Repositories;

public class GenericRepository<T>(AppDbContext context) : IGenericRepository<T> where T : BaseEntity
{
	protected readonly AppDbContext Context = context;
	protected readonly DbSet<T> Set = context.Set<T>();

	public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
		await Set.FindAsync([id], ct);

	public async Task<IReadOnlyList<T>> GetAllAsync(Expression<Func<T, bool>>? filtre = null, CancellationToken ct = default)
	{
		var sorgu = Set.AsNoTracking();
		if (filtre is not null)
			sorgu = sorgu.Where(filtre);

		return await sorgu.ToListAsync(ct);
	}

	public Task<bool> AnyAsync(Expression<Func<T, bool>> filtre, CancellationToken ct = default) =>
		Set.AnyAsync(filtre, ct);

	public async Task AddAsync(T entity, CancellationToken ct = default) =>
		await Set.AddAsync(entity, ct);

	public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default) =>
		Set.AddRangeAsync(entities, ct);

	public void Update(T entity) => Set.Update(entity);

	public void Remove(T entity) => Set.Remove(entity);
}
