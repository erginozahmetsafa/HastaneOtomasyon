using System.Collections.Generic;

namespace HastaneOtomasyon.Core.Entities
{
	// Tek klinik veya çok şubeli kurumun bir şubesi.
	public class Hastane
	{
		public int Id { get; set; }
		public string Ad { get; set; }
		public string Adres { get; set; }
		public string Telefon { get; set; }

		public ICollection<Poliklinik> Poliklinikler { get; set; } = new List<Poliklinik>();
		public ICollection<Calisan> Calisanlar { get; set; } = new List<Calisan>();
		public ICollection<CalismaSaati> CalismaSaatleri { get; set; } = new List<CalismaSaati>();
	}
}
