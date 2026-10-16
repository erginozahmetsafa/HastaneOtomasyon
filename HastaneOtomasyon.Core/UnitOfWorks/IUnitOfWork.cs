namespace HastaneOtomasyon.Core.UnitOfWorks;

// Repository'lerde yapılan değişiklikleri tek bir işlem olarak veritabanına yazar.
public interface IUnitOfWork
{
	Task<int> SaveChangesAsync(CancellationToken ct = default);

	// İşlemi bir transaction içinde, verilen kaynakları münhasır kilitleyerek çalıştırır.
	// Aynı kaynağı isteyen diğer işlemler bu işlem bitene kadar bekler; böylece
	// "kontrol et → kaydet" adımları arasına başka bir istek giremez.
	// Kilit alınamazsa EszamanliIslemException fırlatılır.
	Task KilitliIslemAsync(IEnumerable<string> kaynaklar, Func<Task> islem, CancellationToken ct = default);
}
