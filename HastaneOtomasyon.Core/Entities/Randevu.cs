using HastaneOtomasyon.Core.Enums;

namespace HastaneOtomasyon.Core.Entities;

public class Randevu : BaseEntity
{
	public int HastaId { get; set; }
	public Hasta Hasta { get; set; } = null!;
	public int DoktorId { get; set; }
	public Doktor Doktor { get; set; } = null!;
	public int HizmetId { get; set; }
	public Hizmet Hizmet { get; set; } = null!;

	// Çakışma kontrolü için başlangıç ve bitiş birlikte tutulur.
	public DateTime Baslangic { get; set; }
	public DateTime Bitis { get; set; }

	// Randevu anındaki süre ve ücret; hizmet sonradan değişse de kayıt korunur.
	public int SureDakika { get; set; }
	public decimal Ucret { get; set; }

	// Onay mekanizması
	public RandevuDurum Durum { get; set; } = RandevuDurum.Bekliyor;
	public DateTime? OnayTarihi { get; set; }
	public int? OnaylayanId { get; set; } // Doktor veya Yönetici
	public Calisan? Onaylayan { get; set; }
	public string? IptalRedNedeni { get; set; }
	public string? HastaNotu { get; set; }
}
