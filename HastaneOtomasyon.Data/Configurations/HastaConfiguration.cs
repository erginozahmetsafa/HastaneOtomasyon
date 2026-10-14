using HastaneOtomasyon.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HastaneOtomasyon.Data.Configurations;

public class HastaConfiguration : IEntityTypeConfiguration<Hasta>
{
	public void Configure(EntityTypeBuilder<Hasta> builder)
	{
		builder.Property(h => h.DogumTarihi).HasColumnType("date");
		builder.Property(h => h.Cinsiyet).HasMaxLength(10);
		builder.Property(h => h.KanGrubu).HasMaxLength(10);
	}
}
