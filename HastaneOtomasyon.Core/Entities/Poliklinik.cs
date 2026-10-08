using System.Collections.Generic;

namespace HastaneOtomasyon.Core.Entities
{
	// Hastane bölümü (Kardiyoloji, Radyoloji, Laboratuvar vb.)
	public class Poliklinik
	{
		public int Id { get; set; }
		public string Ad { get; set; }
		public string Aciklama { get; set; }
		public int HastaneId { get; set; } // Foreign Key
		public Hastane Hastane { get; set; } // Navigation Property

		public ICollection<Doktor> Doktorlar { get; set; } = new List<Doktor>();
		public ICollection<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();
	}
}
