using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Enums;
using HastaneOtomasyon.Core.Exceptions;
using HastaneOtomasyon.UnitTests.Yardimcilar;

namespace HastaneOtomasyon.UnitTests.Entities;

// Randevunun durum geçişleri (onay mekanizması) entity içinde kapsüllenmiştir.
public class RandevuTests
{
	private const int OnaylayanId = 10;
	private static readonly DateTime Simdi = TestVerisi.Simdi;
	private static readonly DateTime Yarin = Simdi.AddDays(1);

	[Fact]
	public void Olustur_SureVeUcretHizmettenAlinir_BekliyorDurumundaBaslar()
	{
		var hizmet = TestVerisi.Hizmet(sureDakika: 45, ucret: 750m);

		var randevu = Randevu.Olustur(hastaId: 5, doktorId: 10, hizmet, ucret: 600m, Yarin, "Baş ağrısı");

		Assert.Equal(Yarin.AddMinutes(45), randevu.Bitis);
		Assert.Equal(45, randevu.SureDakika);
		Assert.Equal(600m, randevu.Ucret);
		Assert.Equal(RandevuDurum.Bekliyor, randevu.Durum);
		Assert.Equal("Baş ağrısı", randevu.HastaNotu);
	}

	[Fact]
	public void Olustur_HizmetSonradanDegisirse_RandevuKaydiEtkilenmez()
	{
		var hizmet = TestVerisi.Hizmet(sureDakika: 30, ucret: 500m);
		var randevu = Randevu.Olustur(5, 10, hizmet, hizmet.Ucret, Yarin, null);

		hizmet.SureDakika = 60;
		hizmet.Ucret = 900m;

		Assert.Equal(30, randevu.SureDakika);
		Assert.Equal(500m, randevu.Ucret);
	}

	[Theory]
	[InlineData(RandevuDurum.Bekliyor, true)]
	[InlineData(RandevuDurum.Onaylandi, true)]
	[InlineData(RandevuDurum.Reddedildi, false)]
	[InlineData(RandevuDurum.IptalEdildi, false)]
	[InlineData(RandevuDurum.Tamamlandi, false)]
	public void AktifMi_YalnizcaBekleyenVeOnayliRandevulardaTrue(RandevuDurum durum, bool beklenen)
	{
		var randevu = TestVerisi.Randevu(Yarin, durum);

		Assert.Equal(beklenen, randevu.AktifMi);
	}

	[Fact]
	public void Onayla_BekleyenRandevu_OnaylanirVeOnaylayanKaydedilir()
	{
		var randevu = TestVerisi.Randevu(Yarin);

		randevu.Onayla(OnaylayanId, Simdi);

		Assert.Equal(RandevuDurum.Onaylandi, randevu.Durum);
		Assert.Equal(OnaylayanId, randevu.OnaylayanId);
		Assert.Equal(Simdi, randevu.OnayTarihi);
	}

	[Theory]
	[InlineData(RandevuDurum.Onaylandi)]
	[InlineData(RandevuDurum.Reddedildi)]
	[InlineData(RandevuDurum.IptalEdildi)]
	[InlineData(RandevuDurum.Tamamlandi)]
	public void Onayla_BekleyenDegilse_HataVerir(RandevuDurum durum)
	{
		var randevu = TestVerisi.Randevu(Yarin, durum);

		Assert.Throws<IsKuraliException>(() => randevu.Onayla(OnaylayanId, Simdi));
		Assert.Equal(durum, randevu.Durum);
	}

	[Fact]
	public void Onayla_BaslamisRandevu_HataVerir()
	{
		var randevu = TestVerisi.Randevu(Simdi.AddMinutes(-5));

		Assert.Throws<IsKuraliException>(() => randevu.Onayla(OnaylayanId, Simdi));
	}

	[Fact]
	public void Reddet_BekleyenRandevu_ReddedilirVeNedenKirpilir()
	{
		var randevu = TestVerisi.Randevu(Yarin);

		randevu.Reddet(OnaylayanId, "  Doktor izinli  ", Simdi);

		Assert.Equal(RandevuDurum.Reddedildi, randevu.Durum);
		Assert.Equal("Doktor izinli", randevu.IptalRedNedeni);
		Assert.Equal(OnaylayanId, randevu.OnaylayanId);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Reddet_NedenBos_HataVerir(string? neden)
	{
		var randevu = TestVerisi.Randevu(Yarin);

		Assert.Throws<IsKuraliException>(() => randevu.Reddet(OnaylayanId, neden!, Simdi));
		Assert.Equal(RandevuDurum.Bekliyor, randevu.Durum);
	}

	[Fact]
	public void Reddet_OnayliRandevu_HataVerir()
	{
		var randevu = TestVerisi.Randevu(Yarin, RandevuDurum.Onaylandi);

		Assert.Throws<IsKuraliException>(() => randevu.Reddet(OnaylayanId, "Neden", Simdi));
	}

	[Theory]
	[InlineData(RandevuDurum.Bekliyor)]
	[InlineData(RandevuDurum.Onaylandi)]
	public void IptalEt_AktifRandevu_IptalEdilir(RandevuDurum durum)
	{
		var randevu = TestVerisi.Randevu(Yarin, durum);

		randevu.IptalEt(" Gelemeyeceğim ", Simdi);

		Assert.Equal(RandevuDurum.IptalEdildi, randevu.Durum);
		Assert.Equal("Gelemeyeceğim", randevu.IptalRedNedeni);
	}

	[Fact]
	public void IptalEt_NedenZorunluDegildir()
	{
		var randevu = TestVerisi.Randevu(Yarin);

		randevu.IptalEt(null, Simdi);

		Assert.Equal(RandevuDurum.IptalEdildi, randevu.Durum);
		Assert.Null(randevu.IptalRedNedeni);
	}

	[Theory]
	[InlineData(RandevuDurum.Reddedildi)]
	[InlineData(RandevuDurum.IptalEdildi)]
	[InlineData(RandevuDurum.Tamamlandi)]
	public void IptalEt_AktifDegilse_HataVerir(RandevuDurum durum)
	{
		var randevu = TestVerisi.Randevu(Yarin, durum);

		Assert.Throws<IsKuraliException>(() => randevu.IptalEt(null, Simdi));
	}

	[Fact]
	public void IptalEt_BaslamisRandevu_HataVerir()
	{
		var randevu = TestVerisi.Randevu(Simdi, RandevuDurum.Onaylandi);

		Assert.Throws<IsKuraliException>(() => randevu.IptalEt(null, Simdi));
	}

	[Fact]
	public void Tamamla_OnayliVeBaslamisRandevu_Tamamlanir()
	{
		var randevu = TestVerisi.Randevu(Simdi.AddMinutes(-10), RandevuDurum.Onaylandi);

		randevu.Tamamla(Simdi);

		Assert.Equal(RandevuDurum.Tamamlandi, randevu.Durum);
	}

	[Fact]
	public void Tamamla_HenuzBaslamamisRandevu_HataVerir()
	{
		var randevu = TestVerisi.Randevu(Yarin, RandevuDurum.Onaylandi);

		Assert.Throws<IsKuraliException>(() => randevu.Tamamla(Simdi));
	}

	[Theory]
	[InlineData(RandevuDurum.Bekliyor)]
	[InlineData(RandevuDurum.Reddedildi)]
	[InlineData(RandevuDurum.IptalEdildi)]
	[InlineData(RandevuDurum.Tamamlandi)]
	public void Tamamla_OnayliDegilse_HataVerir(RandevuDurum durum)
	{
		var randevu = TestVerisi.Randevu(Simdi.AddMinutes(-10), durum);

		Assert.Throws<IsKuraliException>(() => randevu.Tamamla(Simdi));
	}
}
