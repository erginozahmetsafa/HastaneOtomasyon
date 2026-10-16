using HastaneOtomasyon.Core.Exceptions;
using HastaneOtomasyon.Core.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace HastaneOtomasyon.Data.UnitOfWorks;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
	private const int KilitZamanAsimiMs = 10_000;

	public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
		context.SaveChangesAsync(ct);

	// SQL Server uygulama kilidi (sp_getapplock) kullanır. Kilit transaction'a bağlıdır;
	// commit/rollback ile birlikte otomatik bırakılır.
	public async Task KilitliIslemAsync(IEnumerable<string> kaynaklar, Func<Task> islem, CancellationToken ct = default)
	{
		await using var transaction = await context.Database.BeginTransactionAsync(ct);

		// Kaynaklar her işlemde aynı sırayla kilitlenir; iki işlemin birbirini sonsuza dek beklemesi (deadlock) önlenir.
		foreach (var kaynak in kaynaklar.Distinct().Order(StringComparer.Ordinal))
		{
			var sonuc = (await context.Database.SqlQuery<int>($"""
				DECLARE @sonuc int;
				EXEC @sonuc = sp_getapplock @Resource = {kaynak}, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = {KilitZamanAsimiMs};
				SELECT @sonuc AS [Value];
				""").ToListAsync(ct)).Single();

			// 0 ve 1 başarı; negatif değerler zaman aşımı/iptal/kilitlenme demektir.
			if (sonuc < 0)
				throw new EszamanliIslemException();
		}

		await islem();
		await transaction.CommitAsync(ct);
	}
}
