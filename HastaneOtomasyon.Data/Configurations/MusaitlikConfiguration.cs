using HastaneOtomasyon.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HastaneOtomasyon.Data.Configurations;

public class MusaitlikConfiguration : IEntityTypeConfiguration<Musaitlik>
{
	public void Configure(EntityTypeBuilder<Musaitlik> builder)
	{
		builder.ToTable("Musaitlikler", t =>
			t.HasCheckConstraint("CK_Musaitlik_Aralik", "[Baslangic] < [Bitis]"));

		// MusaitMiAsync sorgusu bu indeksi kullanır.
		builder.HasIndex(m => new { m.DoktorId, m.Gun });

		builder.HasOne(m => m.Doktor)
			.WithMany(d => d.Musaitlikler)
			.HasForeignKey(m => m.DoktorId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
