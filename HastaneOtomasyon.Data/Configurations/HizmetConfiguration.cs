using HastaneOtomasyon.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HastaneOtomasyon.Data.Configurations;

public class HizmetConfiguration : IEntityTypeConfiguration<Hizmet>
{
	public void Configure(EntityTypeBuilder<Hizmet> builder)
	{
		builder.ToTable("Hizmetler", t =>
		{
			t.HasCheckConstraint("CK_Hizmet_SureDakika", "[SureDakika] > 0");
			t.HasCheckConstraint("CK_Hizmet_Ucret", "[Ucret] >= 0");
		});

		builder.Property(h => h.Ad).HasMaxLength(100);
		builder.Property(h => h.Aciklama).HasMaxLength(500);

		builder.HasOne(h => h.Poliklinik)
			.WithMany(p => p.Hizmetler)
			.HasForeignKey(h => h.PoliklinikId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
