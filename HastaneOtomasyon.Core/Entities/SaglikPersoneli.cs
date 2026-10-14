using HastaneOtomasyon.Core.Enums;

namespace HastaneOtomasyon.Core.Entities;

// Hemşire, laborant, radyoloji teknisyeni vb.
public class SaglikPersoneli : Calisan
{
	public override Roller Rol => Roller.SaglikPersoneli;

	public string Gorev { get; set; } = string.Empty;
	public int? PoliklinikId { get; set; }
	public Poliklinik? Poliklinik { get; set; }
}
