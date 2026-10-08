using HastaneOtomasyon.Core.Enums;

namespace HastaneOtomasyon.Core.Entities
{
	// Tüm kullanıcıların ortak atası. Doğrudan örneklenemez.
	public abstract class Kisi
	{
		public int Id { get; set; }
		public string TcKimlikNo { get; set; }
		public string Ad { get; set; }
		public string Soyad { get; set; }
		public string Email { get; set; }
		public string Telefon { get; set; }
		public string SifreHash { get; set; }

		// Her alt sınıf kendi rolünü belirtir (çok biçimlilik).
		public abstract Roller Rol { get; }
	}
}
