namespace HastaneOtomasyon.Core.Entities
{
	// Hastanenin/kliniğin açık olduğu saatler.
	public class CalismaSaati : ZamanDilimi
	{
		public int HastaneId { get; set; } // Foreign Key
		public Hastane Hastane { get; set; } // Navigation Property
	}
}
