using HastaneOtomasyon.Core.Enums;
using HastaneOtomasyon.Core.Exceptions;

namespace HastaneOtomasyon.Core.Entities;

// Tüm kullanıcıların ortak atası. Doğrudan örneklenemez.
public abstract class Kisi : BaseEntity
{
	public string TcKimlikNo { get; set; } = string.Empty;
	public string Ad { get; set; } = string.Empty;
	public string Soyad { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string? Telefon { get; set; }
	public string SifreHash { get; set; } = string.Empty;

	// Pasif hesap giriş yapamaz; açık oturumları da geçersiz olur.
	public bool AktifMi { get; set; } = true;

	// Token'a yazılır. Değiştiğinde o ana kadar verilmiş tüm token'lar geçersiz olur.
	public Guid GuvenlikDamgasi { get; set; } = Guid.NewGuid();

	// Her alt sınıf kendi rolünü belirtir (çok biçimlilik).
	public abstract Roller Rol { get; }

	public void SifreyiDegistir(string yeniSifreHash)
	{
		SifreHash = yeniSifreHash;
		GuvenlikDamgasi = Guid.NewGuid(); // diğer cihazlardaki oturumlar kapanır
	}

	public void Pasiflestir()
	{
		if (!AktifMi)
			throw new IsKuraliException("Hesap zaten pasif durumda.");
		AktifMi = false;
	}

	public void Aktiflestir()
	{
		if (AktifMi)
			throw new IsKuraliException("Hesap zaten aktif durumda.");
		AktifMi = true;
	}
}
