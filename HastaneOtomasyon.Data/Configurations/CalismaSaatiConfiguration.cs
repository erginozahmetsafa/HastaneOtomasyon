using HastaneOtomasyon.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HastaneOtomasyon.Data.Configurations;

public class CalismaSaatiConfiguration : IEntityTypeConfiguration<CalismaSaati>
{
	public void Configure(EntityTypeBuilder<CalismaSaati> builder)
	{
		builder.ToTable("CalismaSaatleri", t =>
			t.HasCheckConstraint("CK_CalismaSaati_Aralik", "[Baslangic] < [Bitis]"));

		builder.HasIndex(c => new { c.HastaneId, c.Gun });

		builder.HasOne(c => c.Hastane)
			.WithMany(h => h.CalismaSaatleri)
			.HasForeignKey(c => c.HastaneId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
