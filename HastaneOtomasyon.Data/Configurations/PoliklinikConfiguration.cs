using HastaneOtomasyon.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HastaneOtomasyon.Data.Configurations;

public class PoliklinikConfiguration : IEntityTypeConfiguration<Poliklinik>
{
	public void Configure(EntityTypeBuilder<Poliklinik> builder)
	{
		builder.ToTable("Poliklinikler");

		builder.Property(p => p.Ad).HasMaxLength(100);
		builder.Property(p => p.Aciklama).HasMaxLength(500);

		// Aynı hastanede aynı isimde iki bölüm olamaz.
		builder.HasIndex(p => new { p.HastaneId, p.Ad }).IsUnique();

		builder.HasOne(p => p.Hastane)
			.WithMany(h => h.Poliklinikler)
			.HasForeignKey(p => p.HastaneId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
