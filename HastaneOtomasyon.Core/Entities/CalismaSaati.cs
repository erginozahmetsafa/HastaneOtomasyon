namespace HastaneOtomasyon.Core.Entities;

// Hastanenin/kliniğin açık olduğu saatler.
public class CalismaSaati : ZamanDilimi
{
	public int HastaneId { get; set; }
	public Hastane Hastane { get; set; } = null!;
}
