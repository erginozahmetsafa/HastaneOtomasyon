namespace HastaneOtomasyon.Core.Entities;

// Kurumun sunduğu sağlık hizmeti (genel muayene, MR, kan tahlili vb.)
public class Hizmet : BaseEntity
{
	public string Ad { get; set; } = string.Empty;
	public string? Aciklama { get; set; }
	public int SureDakika { get; set; }
	public decimal Ucret { get; set; }
	public bool AktifMi { get; set; } = true;
	public int PoliklinikId { get; set; }
	public Poliklinik Poliklinik { get; set; } = null!;

	public ICollection<DoktorHizmet> DoktorHizmetler { get; set; } = [];
}
