using HastaneOtomasyon.Core.Enums;
using System;
using System.Collections.Generic;

namespace HastaneOtomasyon.Core.Entities
{
	// Hasta bir hastaneye bağlı değildir; tüm şubelerden randevu alabilir.
	public class Hasta : Kisi
	{
		public override Roller Rol => Roller.Hasta;

		public DateTime DogumTarihi { get; set; }
		public string Cinsiyet { get; set; }
		public string KanGrubu { get; set; }

		public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
	}
}
