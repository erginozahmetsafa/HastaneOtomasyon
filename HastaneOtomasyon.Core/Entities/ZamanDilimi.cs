namespace HastaneOtomasyon.Core.Entities;

// Haftalık tekrar eden bir zaman aralığı. CalismaSaati ve Musaitlik bundan türer.
public abstract class ZamanDilimi : BaseEntity
{
	public DayOfWeek Gun { get; set; }
	public TimeSpan Baslangic { get; set; }
	public TimeSpan Bitis { get; set; }

	// Verilen aralık bu dilimin tamamen içinde mi?
	public bool Kapsar(TimeSpan baslangic, TimeSpan bitis) =>
		Baslangic <= baslangic && bitis <= Bitis;
}
