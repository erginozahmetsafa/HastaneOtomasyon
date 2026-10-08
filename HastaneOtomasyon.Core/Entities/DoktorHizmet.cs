namespace HastaneOtomasyon.Core.Entities
{
	// Doktor ile Hizmet arasındaki çoktan çoğa ilişki tablosu.
	public class DoktorHizmet
	{
		public int DoktorId { get; set; } // Foreign Key
		public Doktor Doktor { get; set; } // Navigation Property
		public int HizmetId { get; set; } // Foreign Key
		public Hizmet Hizmet { get; set; } // Navigation Property

		// Doktora özel ücret; boşsa Hizmet.Ucret geçerlidir.
		public decimal? OzelUcret { get; set; }
	}
}
