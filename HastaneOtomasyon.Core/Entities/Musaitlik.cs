namespace HastaneOtomasyon.Core.Entities;

// Doktorun randevu kabul ettiği zaman dilimi. Hastalar yalnızca bu aralıklara randevu alabilir.
public class Musaitlik : ZamanDilimi
{
	public bool AktifMi { get; set; } = true;
	public int DoktorId { get; set; }
	public Doktor Doktor { get; set; } = null!;
}
