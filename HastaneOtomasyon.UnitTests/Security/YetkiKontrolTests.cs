using HastaneOtomasyon.Core.Enums;
using HastaneOtomasyon.Core.Exceptions;
using HastaneOtomasyon.Service.Security;
using HastaneOtomasyon.UnitTests.Yardimcilar;

namespace HastaneOtomasyon.UnitTests.Security;

public class YetkiKontrolTests
{
	private const int HastaneId = TestVerisi.HastaneId;
	private const int DigerHastaneId = TestVerisi.DigerHastaneId;

	[Fact]
	public void GirisYapmisOlmali_GirisYapilmis_IdDoner()
	{
		Assert.Equal(7, TestKullanici.HastaOlarak(7).GirisYapmisOlmali());
	}

	[Fact]
	public void GirisYapmisOlmali_Anonim_HataVerir()
	{
		Assert.Throws<YetkisizIslemException>(() => TestKullanici.Anonim.GirisYapmisOlmali());
	}

	[Fact]
	public void RolOlmali_DogruRol_IdDoner()
	{
		Assert.Equal(7, TestKullanici.HastaOlarak(7).RolOlmali(Roller.Hasta));
	}

	[Fact]
	public void RolOlmali_FarkliRol_HataVerir()
	{
		var doktor = TestKullanici.DoktorOlarak(7, HastaneId);

		Assert.Throws<YetkisizIslemException>(() => doktor.RolOlmali(Roller.Hasta));
	}

	[Fact]
	public void RolOlmali_SistemYoneticisiBileBaskaRolunYerineGecemez()
	{
		Assert.Throws<YetkisizIslemException>(() => TestKullanici.SistemYoneticisiOlarak().RolOlmali(Roller.Hasta));
	}

	[Fact]
	public void RolOlmali_Anonim_HataVerir()
	{
		Assert.Throws<YetkisizIslemException>(() => TestKullanici.Anonim.RolOlmali(Roller.Hasta));
	}

	public static TheoryData<TestKullanici, bool> HastaneYoneticiligi => new()
	{
		{ TestKullanici.SistemYoneticisiOlarak(), true },          // tüm şubeler
		{ TestKullanici.YoneticiOlarak(2, HastaneId), true },      // kendi hastanesi
		{ TestKullanici.YoneticiOlarak(2, DigerHastaneId), false }, // başka hastane
		{ TestKullanici.DoktorOlarak(3, HastaneId), false },
		{ TestKullanici.PersonelOlarak(4, HastaneId), false },
		{ TestKullanici.HastaOlarak(5), false },
		{ TestKullanici.Anonim, false }
	};

	[Theory]
	[MemberData(nameof(HastaneYoneticiligi))]
	public void HastaneYoneticisiMi(TestKullanici kullanici, bool beklenen)
	{
		Assert.Equal(beklenen, kullanici.HastaneYoneticisiMi(HastaneId));
	}

	[Fact]
	public void HastaneYonetebilmeli_YetkisizKullanici_HataVerir()
	{
		var baskaHastaneninYoneticisi = TestKullanici.YoneticiOlarak(2, DigerHastaneId);

		Assert.Throws<YetkisizIslemException>(() => baskaHastaneninYoneticisi.HastaneYonetebilmeli(HastaneId));
	}

	public static TheoryData<TestKullanici, bool> DoktorYonetimi => new()
	{
		{ TestKullanici.DoktorOlarak(10, HastaneId), true },        // doktorun kendisi
		{ TestKullanici.DoktorOlarak(11, HastaneId), false },       // aynı hastanedeki başka doktor
		{ TestKullanici.YoneticiOlarak(2, HastaneId), true },       // doktorun hastanesinin yöneticisi
		{ TestKullanici.YoneticiOlarak(2, DigerHastaneId), false },
		{ TestKullanici.SistemYoneticisiOlarak(), true },
		{ TestKullanici.PersonelOlarak(4, HastaneId), false },
		{ TestKullanici.HastaOlarak(10), false }                    // Id aynı olsa da rolü hasta
	};

	[Theory]
	[MemberData(nameof(DoktorYonetimi))]
	public void DoktorYonetebilmeli(TestKullanici kullanici, bool izinli)
	{
		var doktor = TestVerisi.Doktor(id: 10, hastaneId: HastaneId);

		var hata = Record.Exception(() => kullanici.DoktorYonetebilmeli(doktor));

		if (izinli)
			Assert.Null(hata);
		else
			Assert.IsType<YetkisizIslemException>(hata);
	}
}
