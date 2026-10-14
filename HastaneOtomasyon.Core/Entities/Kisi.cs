using HastaneOtomasyon.Core.Enums;

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

	// Her alt sınıf kendi rolünü belirtir (çok biçimlilik).
	public abstract Roller Rol { get; }
}
