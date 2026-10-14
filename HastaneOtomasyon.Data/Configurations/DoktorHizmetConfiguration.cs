using HastaneOtomasyon.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HastaneOtomasyon.Data.Configurations;

public class DoktorHizmetConfiguration : IEntityTypeConfiguration<DoktorHizmet>
{
	public void Configure(EntityTypeBuilder<DoktorHizmet> builder)
	{
		builder.ToTable("DoktorHizmetler");

		builder.HasKey(dh => new { dh.DoktorId, dh.HizmetId });

		builder.HasOne(dh => dh.Doktor)
			.WithMany(d => d.DoktorHizmetler)
			.HasForeignKey(dh => dh.DoktorId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(dh => dh.Hizmet)
			.WithMany(h => h.DoktorHizmetler)
			.HasForeignKey(dh => dh.HizmetId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
