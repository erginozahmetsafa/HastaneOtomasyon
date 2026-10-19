using System.ComponentModel.DataAnnotations;
using HastaneOtomasyon.Core.DTOs;

namespace HastaneOtomasyon.UnitTests.DTOs;

// API'de [ApiController] bu kuralları otomatik uygular; burada doğrudan Validator ile denenir.
public class DtoDogrulamaTests
{
	private static HastaKayitDto GecerliKayit(string sifre = "Sifre123", string tcKimlikNo = "12345678901") => new()
	{
		TcKimlikNo = tcKimlikNo,
		Ad = "Mehmet",
		Soyad = "Demir",
		Email = "mehmet@ornek.com",
		Sifre = sifre,
		DogumTarihi = new DateTime(1990, 1, 1)
	};

	private static List<string> HataliAlanlar(object dto)
	{
		var sonuclar = new List<ValidationResult>();
		Validator.TryValidateObject(dto, new ValidationContext(dto), sonuclar, validateAllProperties: true);
		return sonuclar.SelectMany(s => s.MemberNames).ToList();
	}

	[Fact]
	public void HastaKayit_GecerliVeri_HataYok()
	{
		Assert.Empty(HataliAlanlar(GecerliKayit()));
	}

	[Theory]
	[InlineData("Sifre123")]
	[InlineData("abcdefg1")]
	[InlineData("Güçlü2026")]   // Türkçe karakterler harf sayılır
	[InlineData("ışık1234")]
	public void SifrePolitikasi_GecerliSifreler(string sifre)
	{
		Assert.DoesNotContain(nameof(HastaKayitDto.Sifre), HataliAlanlar(GecerliKayit(sifre)));
	}

	[Theory]
	[InlineData("")]
	[InlineData("Ab1")]          // 8 karakterden kısa
	[InlineData("sadeceharf")]   // rakam yok
	[InlineData("12345678")]     // harf yok
	[InlineData("!!!!????")]     // ne harf ne rakam
	public void SifrePolitikasi_GecersizSifreler(string sifre)
	{
		Assert.Contains(nameof(HastaKayitDto.Sifre), HataliAlanlar(GecerliKayit(sifre)));
	}

	[Fact]
	public void SifreDegistir_YeniSifreAyniPolitikayaTabidir()
	{
		var dto = new SifreDegistirDto { MevcutSifre = "Sifre123", YeniSifre = "kisa" };

		Assert.Contains(nameof(SifreDegistirDto.YeniSifre), HataliAlanlar(dto));
	}

	[Theory]
	[InlineData("1234567890")]    // 10 hane
	[InlineData("123456789012")]  // 12 hane
	[InlineData("1234567890a")]   // harf içeriyor
	public void TcKimlikNo_11HaneliRakamOlmali(string tcKimlikNo)
	{
		Assert.Contains(nameof(HastaKayitDto.TcKimlikNo), HataliAlanlar(GecerliKayit(tcKimlikNo: tcKimlikNo)));
	}
}
