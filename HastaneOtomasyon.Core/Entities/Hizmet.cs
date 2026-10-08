using System.Collections.Generic;

namespace HastaneOtomasyon.Core.Entities
{
	// Kurumun sunduğu sağlık hizmeti (genel muayene, MR, kan tahlili vb.)
	public class Hizmet
	{
		public int Id { get; set; }
		public string Ad { get; set; }
		public string Aciklama { get; set; }
		public int SureDakika { get; set; }
		public decimal Ucret { get; set; }
		public bool AktifMi { get; set; } = true;
		public int PoliklinikId { get; set; } // Foreign Key
		public Poliklinik Poliklinik { get; set; } // Navigation Property

		public ICollection<DoktorHizmet> DoktorHizmetler { get; set; } = new List<DoktorHizmet>();
	}
}
