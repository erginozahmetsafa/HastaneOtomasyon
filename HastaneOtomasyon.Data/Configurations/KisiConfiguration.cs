using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HastaneOtomasyon.Data.Configurations;

// Kişi hiyerarşisi tek tabloda tutulur (TPH); ayırt edici sütun rol enum'udur.
public class KisiConfiguration : IEntityTypeConfiguration<Kisi>
{
	public void Configure(EntityTypeBuilder<Kisi> builder)
	{
		builder.ToTable("Kisiler");

		builder.HasDiscriminator<Roller>("KisiTipi")
			.HasValue<Hasta>(Roller.Hasta)
			.HasValue<Doktor>(Roller.Doktor)
			.HasValue<SaglikPersoneli>(Roller.SaglikPersoneli)
			.HasValue<Yonetici>(Roller.Yonetici)
			.HasValue<SistemYoneticisi>(Roller.SistemYoneticisi);

		builder.Ignore(k => k.Rol);

		builder.Property(k => k.TcKimlikNo).HasMaxLength(11).IsFixedLength();
		builder.Property(k => k.Ad).HasMaxLength(50);
		builder.Property(k => k.Soyad).HasMaxLength(50);
		builder.Property(k => k.Email).HasMaxLength(100);
		builder.Property(k => k.Telefon).HasMaxLength(20);
		builder.Property(k => k.SifreHash).HasMaxLength(256);

		builder.HasIndex(k => k.TcKimlikNo).IsUnique();
		builder.HasIndex(k => k.Email).IsUnique();
	}
}
