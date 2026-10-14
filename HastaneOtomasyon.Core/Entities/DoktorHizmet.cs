namespace HastaneOtomasyon.Core.Entities;

// Doktor ile Hizmet arasındaki çoktan çoğa ilişki. Anahtar: (DoktorId, HizmetId).
public class DoktorHizmet
{
	public int DoktorId { get; set; }
	public Doktor Doktor { get; set; } = null!;
	public int HizmetId { get; set; }
	public Hizmet Hizmet { get; set; } = null!;

	// Doktora özel ücret; boşsa Hizmet.Ucret geçerlidir.
	public decimal? OzelUcret { get; set; }
}
