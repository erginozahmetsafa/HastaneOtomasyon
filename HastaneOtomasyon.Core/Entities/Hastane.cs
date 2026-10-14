namespace HastaneOtomasyon.Core.Entities;

// Tek klinik veya çok şubeli kurumun bir şubesi.
public class Hastane : BaseEntity
{
	public string Ad { get; set; } = string.Empty;
	public string Adres { get; set; } = string.Empty;
	public string? Telefon { get; set; }

	public ICollection<Poliklinik> Poliklinikler { get; set; } = [];
	public ICollection<Calisan> Calisanlar { get; set; } = [];
	public ICollection<CalismaSaati> CalismaSaatleri { get; set; } = [];
}
