using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Enums;
using RandevuVarligi = HastaneOtomasyon.Core.Entities.Randevu;

namespace HastaneOtomasyon.UnitTests.Yardimcilar;

// Testlerde kullanılan örnek varlıklar. Sabit tarih sayesinde testler her gün aynı sonucu verir.
public static class TestVerisi
{
	public const int HastaneId = 1;
	public const int DigerHastaneId = 2;

	// 19 Ekim 2026, Pazartesi 10:00
	public static readonly DateTime Simdi = new(2026, 10, 19, 10, 0, 0);
	public static readonly DateOnly Bugun = DateOnly.FromDateTime(Simdi);

	public static Hizmet Hizmet(int id = 1, int sureDakika = 30, decimal ucret = 500m) => new()
	{
		Id = id,
		Ad = "Genel Muayene",
		SureDakika = sureDakika,
		Ucret = ucret,
		PoliklinikId = 1
	};

	public static Doktor Doktor(int id = 10, int hastaneId = HastaneId) => new()
	{
		Id = id,
		Ad = "Ayşe",
		Soyad = "Yılmaz",
		Unvan = "Uzm. Dr.",
		UzmanlikAlani = "Dahiliye",
		HastaneId = hastaneId,
		PoliklinikId = 1,
		Poliklinik = new Poliklinik { Id = 1, Ad = "Dahiliye", HastaneId = hastaneId }
	};

	public static Hasta Hasta(int id = 5) => new()
	{
		Id = id,
		Ad = "Mehmet",
		Soyad = "Demir",
		Email = "mehmet@ornek.com"
	};

	public static DoktorHizmet DoktorHizmet(Doktor? doktor = null, Hizmet? hizmet = null, decimal? ozelUcret = null)
	{
		doktor ??= Doktor();
		hizmet ??= Hizmet();
		return new DoktorHizmet
		{
			DoktorId = doktor.Id,
			Doktor = doktor,
			HizmetId = hizmet.Id,
			Hizmet = hizmet,
			OzelUcret = ozelUcret
		};
	}

	public static Musaitlik Musaitlik(DayOfWeek gun, int basSaat, int bitSaat, int doktorId = 10, bool aktifMi = true) => new()
	{
		DoktorId = doktorId,
		Gun = gun,
		Baslangic = TimeSpan.FromHours(basSaat),
		Bitis = TimeSpan.FromHours(bitSaat),
		AktifMi = aktifMi
	};

	// Navigasyon özellikleri dolu bir randevu (DTO dönüşümü ve yetki kontrolü bunlara ihtiyaç duyar).
	public static Randevu Randevu(
		DateTime baslangic,
		RandevuDurum durum = RandevuDurum.Bekliyor,
		Hasta? hasta = null,
		Doktor? doktor = null,
		Hizmet? hizmet = null,
		int id = 100)
	{
		hasta ??= Hasta();
		doktor ??= Doktor();
		hizmet ??= Hizmet();

		var randevu = RandevuVarligi.Olustur(hasta.Id, doktor.Id, hizmet, hizmet.Ucret, baslangic, null);
		randevu.Id = id;
		randevu.Durum = durum;
		randevu.Hasta = hasta;
		randevu.Doktor = doktor;
		randevu.Hizmet = hizmet;
		return randevu;
	}
}
