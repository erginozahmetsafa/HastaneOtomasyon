using HastaneOtomasyon.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HastaneOtomasyon.Data.Configurations;

public class DoktorConfiguration : IEntityTypeConfiguration<Doktor>
{
	public void Configure(EntityTypeBuilder<Doktor> builder)
	{
		builder.Property(d => d.Unvan).HasMaxLength(30);
		builder.Property(d => d.UzmanlikAlani).HasMaxLength(100);

		// SaglikPersoneli.PoliklinikId ile aynı sütunu paylaşır (TPH'de mükerrer sütun oluşmasın).
		builder.Property(d => d.PoliklinikId).HasColumnName("PoliklinikId");

		builder.HasOne(d => d.Poliklinik)
			.WithMany(p => p.Doktorlar)
			.HasForeignKey(d => d.PoliklinikId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
