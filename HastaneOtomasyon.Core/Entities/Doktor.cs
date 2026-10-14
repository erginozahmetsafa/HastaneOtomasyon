using HastaneOtomasyon.Core.Enums;

namespace HastaneOtomasyon.Core.Entities;

public class Doktor : Calisan
{
	public override Roller Rol => Roller.Doktor;

	public string? Unvan { get; set; } // Örn: Uzm. Dr., Doç. Dr.
	public string UzmanlikAlani { get; set; } = string.Empty;
	public int PoliklinikId { get; set; }
	public Poliklinik Poliklinik { get; set; } = null!;

	public ICollection<DoktorHizmet> DoktorHizmetler { get; set; } = [];
	public ICollection<Musaitlik> Musaitlikler { get; set; } = [];
	public ICollection<Randevu> Randevular { get; set; } = [];
}
