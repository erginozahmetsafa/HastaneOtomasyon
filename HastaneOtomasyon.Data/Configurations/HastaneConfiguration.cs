using HastaneOtomasyon.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HastaneOtomasyon.Data.Configurations;

public class HastaneConfiguration : IEntityTypeConfiguration<Hastane>
{
	public void Configure(EntityTypeBuilder<Hastane> builder)
	{
		builder.ToTable("Hastaneler");

		builder.Property(h => h.Ad).HasMaxLength(150);
		builder.Property(h => h.Adres).HasMaxLength(500);
		builder.Property(h => h.Telefon).HasMaxLength(20);
	}
}
