using HastaneOtomasyon.Core.Enums;

namespace HastaneOtomasyon.Core.Entities
{
	// Hemşire, laborant, radyoloji teknisyeni vb.
	public class SaglikPersoneli : Calisan
	{
		public override Roller Rol => Roller.SaglikPersoneli;

		public string Gorev { get; set; }
		public int? PoliklinikId { get; set; } // Foreign Key (opsiyonel)
		public Poliklinik Poliklinik { get; set; } // Navigation Property
	}
}
