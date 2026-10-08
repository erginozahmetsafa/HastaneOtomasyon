using System;

namespace HastaneOtomasyon.Core.Entities
{
	// Bir hastaneye/şubeye bağlı çalışanların ortak atası.
	public abstract class Calisan : Kisi
	{
		public DateTime IseBaslamaTarihi { get; set; }
		public int HastaneId { get; set; } // Foreign Key
		public Hastane Hastane { get; set; } // Navigation Property
	}
}
