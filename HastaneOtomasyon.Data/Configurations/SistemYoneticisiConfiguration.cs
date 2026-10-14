using HastaneOtomasyon.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HastaneOtomasyon.Data.Configurations;

public class SistemYoneticisiConfiguration : IEntityTypeConfiguration<SistemYoneticisi>
{
	public void Configure(EntityTypeBuilder<SistemYoneticisi> builder)
	{
		// İlk kurulumda giriş yapılabilmesi için varsayılan hesap.
		// Varsayılan şifre: Admin123!  (canlı ortamda mutlaka değiştirilmelidir)
		builder.HasData(new SistemYoneticisi
		{
			Id = 1,
			TcKimlikNo = "10000000146",
			Ad = "Sistem",
			Soyad = "Yöneticisi",
			Email = "admin@hastane.local",
			SifreHash = "100000.umQQHF/mQNVupUfcYdQhBg==.lhe8lsA0rlUwlub9rCfJCEBaBihmStrWPYvp/52tF9k=",
			OlusturmaTarihi = new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc)
		});
	}
}
