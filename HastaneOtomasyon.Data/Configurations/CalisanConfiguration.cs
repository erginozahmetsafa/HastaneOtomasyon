using HastaneOtomasyon.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HastaneOtomasyon.Data.Configurations;

public class CalisanConfiguration : IEntityTypeConfiguration<Calisan>
{
	public void Configure(EntityTypeBuilder<Calisan> builder)
	{
		builder.HasOne(c => c.Hastane)
			.WithMany(h => h.Calisanlar)
			.HasForeignKey(c => c.HastaneId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
