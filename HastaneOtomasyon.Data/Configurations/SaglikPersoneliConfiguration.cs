using HastaneOtomasyon.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HastaneOtomasyon.Data.Configurations;

public class SaglikPersoneliConfiguration : IEntityTypeConfiguration<SaglikPersoneli>
{
	public void Configure(EntityTypeBuilder<SaglikPersoneli> builder)
	{
		builder.Property(s => s.Gorev).HasMaxLength(50);

		// Doktor.PoliklinikId ile aynı sütunu paylaşır (TPH'de mükerrer sütun oluşmasın).
		builder.Property(s => s.PoliklinikId).HasColumnName("PoliklinikId");

		builder.HasOne(s => s.Poliklinik)
			.WithMany()
			.HasForeignKey(s => s.PoliklinikId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
