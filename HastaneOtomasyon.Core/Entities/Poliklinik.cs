namespace HastaneOtomasyon.Core.Entities;

// Hastane bölümü (Kardiyoloji, Radyoloji, Laboratuvar vb.)
public class Poliklinik : BaseEntity
{
	public string Ad { get; set; } = string.Empty;
	public string? Aciklama { get; set; }
	public int HastaneId { get; set; }
	public Hastane Hastane { get; set; } = null!;

	public ICollection<Doktor> Doktorlar { get; set; } = [];
	public ICollection<Hizmet> Hizmetler { get; set; } = [];
}
