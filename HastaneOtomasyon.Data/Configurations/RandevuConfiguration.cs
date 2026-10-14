using HastaneOtomasyon.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HastaneOtomasyon.Data.Configurations;

public class RandevuConfiguration : IEntityTypeConfiguration<Randevu>
{
	public void Configure(EntityTypeBuilder<Randevu> builder)
	{
		builder.ToTable("Randevular", t =>
			t.HasCheckConstraint("CK_Randevu_Aralik", "[Baslangic] < [Bitis]"));

		builder.Property(r => r.IptalRedNedeni).HasMaxLength(500);
		builder.Property(r => r.HastaNotu).HasMaxLength(500);

		// Çakışma kontrolü ve takvim sorguları bu indeksleri kullanır.
		builder.HasIndex(r => new { r.DoktorId, r.Baslangic });
		builder.HasIndex(r => new { r.HastaId, r.Baslangic });
		builder.HasIndex(r => r.Durum);

		// Randevu geçmişi korunmalı; ilişkili kayıt silinirken randevular silinmez.
		builder.HasOne(r => r.Hasta)
			.WithMany(h => h.Randevular)
			.HasForeignKey(r => r.HastaId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(r => r.Doktor)
			.WithMany(d => d.Randevular)
			.HasForeignKey(r => r.DoktorId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(r => r.Hizmet)
			.WithMany()
			.HasForeignKey(r => r.HizmetId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(r => r.Onaylayan)
			.WithMany()
			.HasForeignKey(r => r.OnaylayanId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
