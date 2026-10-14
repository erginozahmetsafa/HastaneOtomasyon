using HastaneOtomasyon.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HastaneOtomasyon.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<Hastane> Hastaneler => Set<Hastane>();
	public DbSet<Poliklinik> Poliklinikler => Set<Poliklinik>();
	public DbSet<CalismaSaati> CalismaSaatleri => Set<CalismaSaati>();
	public DbSet<Hizmet> Hizmetler => Set<Hizmet>();
	public DbSet<Kisi> Kisiler => Set<Kisi>();
	public DbSet<Hasta> Hastalar => Set<Hasta>();
	public DbSet<Doktor> Doktorlar => Set<Doktor>();
	public DbSet<SaglikPersoneli> SaglikPersonelleri => Set<SaglikPersoneli>();
	public DbSet<Yonetici> Yoneticiler => Set<Yonetici>();
	public DbSet<DoktorHizmet> DoktorHizmetler => Set<DoktorHizmet>();
	public DbSet<Musaitlik> Musaitlikler => Set<Musaitlik>();
	public DbSet<Randevu> Randevular => Set<Randevu>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}

	protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
	{
		// Para alanları için varsayılan hassasiyet
		configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
	}

	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		DenetimAlanlariniDoldur();
		return base.SaveChangesAsync(cancellationToken);
	}

	private void DenetimAlanlariniDoldur()
	{
		var simdi = DateTime.UtcNow;

		foreach (var entry in ChangeTracker.Entries<BaseEntity>())
		{
			switch (entry.State)
			{
				case EntityState.Added:
					entry.Entity.OlusturmaTarihi = simdi;
					break;
				case EntityState.Modified:
					entry.Property(e => e.OlusturmaTarihi).IsModified = false;
					entry.Entity.GuncellemeTarihi = simdi;
					break;
			}
		}
	}
}
