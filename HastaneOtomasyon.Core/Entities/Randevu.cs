using HastaneOtomasyon.Core.Enums;
using System;

namespace HastaneOtomasyon.Core.Entities
{
	public class Randevu
	{
		public int Id { get; set; }

		public int HastaId { get; set; } // Foreign Key
		public Hasta Hasta { get; set; } // Navigation Property
		public int DoktorId { get; set; } // Foreign Key
		public Doktor Doktor { get; set; } // Navigation Property
		public int HizmetId { get; set; } // Foreign Key
		public Hizmet Hizmet { get; set; } // Navigation Property

		// Çakışma kontrolü için başlangıç ve bitiş birlikte tutulur.
		public DateTime Baslangic { get; set; }
		public DateTime Bitis { get; set; }

		// Randevu anındaki süre ve ücret; hizmet sonradan değişse de kayıt korunur.
		public int SureDakika { get; set; }
		public decimal Ucret { get; set; }

		// Onay mekanizması
		public RandevuDurum Durum { get; set; } = RandevuDurum.Bekliyor;
		public DateTime OlusturmaTarihi { get; set; }
		public DateTime? OnayTarihi { get; set; }
		public int? OnaylayanId { get; set; } // Foreign Key (Doktor veya Yönetici)
		public Calisan Onaylayan { get; set; } // Navigation Property
		public string IptalRedNedeni { get; set; }
		public string HastaNotu { get; set; }
	}
}
