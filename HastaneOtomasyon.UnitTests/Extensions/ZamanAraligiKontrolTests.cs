using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Exceptions;
using HastaneOtomasyon.Service.Extensions;

namespace HastaneOtomasyon.UnitTests.Extensions;

public class ZamanAraligiKontrolTests
{
	private static ZamanAraligiDto Aralik(string baslangic, string bitis, DayOfWeek gun = DayOfWeek.Monday) => new()
	{
		Gun = gun,
		Baslangic = TimeSpan.Parse(baslangic),
		Bitis = TimeSpan.Parse(bitis)
	};

	[Theory]
	[InlineData("09:00", "17:00")]
	[InlineData("00:00", "1.00:00")] // 24 saat açık
	public void GecerliOlmali_GecerliAralik_HataVermez(string baslangic, string bitis)
	{
		Aralik(baslangic, bitis).GecerliOlmali();
	}

	[Theory]
	[InlineData("17:00", "09:00")]    // bitiş başlangıçtan önce
	[InlineData("09:00", "09:00")]    // süresiz aralık
	[InlineData("-01:00", "09:00")]   // negatif saat
	[InlineData("22:00", "1.02:00")]  // gün dışına taşıyor
	public void GecerliOlmali_GecersizSaatler_HataVerir(string baslangic, string bitis)
	{
		Assert.Throws<IsKuraliException>(() => Aralik(baslangic, bitis).GecerliOlmali());
	}

	[Fact]
	public void GecerliOlmali_TanimsizGun_HataVerir()
	{
		Assert.Throws<IsKuraliException>(() => Aralik("09:00", "17:00", (DayOfWeek)7).GecerliOlmali());
	}
}
