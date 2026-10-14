namespace HastaneOtomasyon.Core.UnitOfWorks;

// Repository'lerde yapılan değişiklikleri tek bir işlem olarak veritabanına yazar.
public interface IUnitOfWork
{
	Task<int> SaveChangesAsync(CancellationToken ct = default);
}