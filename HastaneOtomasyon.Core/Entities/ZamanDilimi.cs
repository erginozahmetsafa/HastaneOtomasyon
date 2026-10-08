using System;

namespace HastaneOtomasyon.Core.Entities
{
	// Haftalık tekrar eden bir zaman aralığı. CalismaSaati ve Musaitlik bundan türer.
	public abstract class ZamanDilimi
	{
		public int Id { get; set; }
		public DayOfWeek Gun { get; set; }
		public TimeSpan Baslangic { get; set; }
		public TimeSpan Bitis { get; set; }
	}
}
