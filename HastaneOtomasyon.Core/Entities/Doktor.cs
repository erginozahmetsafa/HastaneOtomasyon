using HastaneOtomasyon.Core.Enums;
using System.Collections.Generic;

namespace HastaneOtomasyon.Core.Entities
{
	public class Doktor : Calisan
	{
		public override Roller Rol => Roller.Doktor;

		public string Unvan { get; set; } // Örn: Uzm. Dr., Doç. Dr.
		public string UzmanlikAlani { get; set; }
		public int PoliklinikId { get; set; } // Foreign Key
		public Poliklinik Poliklinik { get; set; } // Navigation Property

		public ICollection<DoktorHizmet> DoktorHizmetler { get; set; } = new List<DoktorHizmet>();
		public ICollection<Musaitlik> Musaitlikler { get; set; } = new List<Musaitlik>();
		public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
	}
}
