using HastaneOtomasyon.Core.UnitOfWorks;

namespace HastaneOtomasyon.Data.UnitOfWorks;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
	public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
		context.SaveChangesAsync(ct);
}
