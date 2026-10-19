using HastaneOtomasyon.Core.Entities;

namespace HastaneOtomasyon.UnitTests.Entities;

public class ZamanDilimiTests
{
	// 09:00 - 12:00
	private static readonly ZamanDilimi Dilim = new Musaitlik
	{
		Gun = DayOfWeek.Monday,
		Baslangic = TimeSpan.FromHours(9),
		Bitis = TimeSpan.FromHours(12)
	};

	[Theory]
	[InlineData("09:00", "09:30", true)]   // başta
	[InlineData("11:30", "12:00", true)]   // sonda
	[InlineData("09:00", "12:00", true)]   // tamamı
	[InlineData("08:45", "09:15", false)]  // başlangıçtan taşıyor
	[InlineData("11:45", "12:15", false)]  // bitişten taşıyor
	[InlineData("13:00", "13:30", false)]  // tamamen dışında
	public void Kapsar_AralikTamamenDilimIcindeyseTrue(string baslangic, string bitis, bool beklenen)
	{
		Assert.Equal(beklenen, Dilim.Kapsar(TimeSpan.Parse(baslangic), TimeSpan.Parse(bitis)));
	}
}
