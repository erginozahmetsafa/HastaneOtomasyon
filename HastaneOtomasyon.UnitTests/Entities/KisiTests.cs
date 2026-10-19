using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Enums;
using HastaneOtomasyon.Core.Exceptions;

namespace HastaneOtomasyon.UnitTests.Entities;

public class KisiTests
{
	public static TheoryData<Kisi, Roller> KisiTurleri => new()
	{
		{ new Hasta(), Roller.Hasta },
		{ new Doktor(), Roller.Doktor },
		{ new SaglikPersoneli(), Roller.SaglikPersoneli },
		{ new Yonetici(), Roller.Yonetici },
		{ new SistemYoneticisi(), Roller.SistemYoneticisi }
	};

	// Çok biçimlilik: Kisi referansı üzerinden çağrılan Rol, gerçek alt sınıfın değerini döner.
	[Theory]
	[MemberData(nameof(KisiTurleri))]
	public void Rol_HerAltSinifKendiRolunuDoner(Kisi kisi, Roller beklenen)
	{
		Assert.Equal(beklenen, kisi.Rol);
	}

	[Fact]
	public void YeniKisi_AktifBaslarVeHerkesFarkliGuvenlikDamgasiAlir()
	{
		var hasta1 = new Hasta();
		var hasta2 = new Hasta();

		Assert.True(hasta1.AktifMi);
		Assert.NotEqual(Guid.Empty, hasta1.GuvenlikDamgasi);
		Assert.NotEqual(hasta1.GuvenlikDamgasi, hasta2.GuvenlikDamgasi);
	}

	[Fact]
	public void SifreyiDegistir_HashGuncellenirVeGuvenlikDamgasiYenilenir()
	{
		var kisi = new Hasta { SifreHash = "eski" };
		var eskiDamga = kisi.GuvenlikDamgasi;

		kisi.SifreyiDegistir("yeni");

		Assert.Equal("yeni", kisi.SifreHash);
		Assert.NotEqual(eskiDamga, kisi.GuvenlikDamgasi); // eski token'lar geçersiz olur
	}

	[Fact]
	public void Pasiflestir_AktifHesap_PasifOlur()
	{
		var doktor = new Doktor();

		doktor.Pasiflestir();

		Assert.False(doktor.AktifMi);
	}

	[Fact]
	public void Pasiflestir_ZatenPasifHesap_HataVerir()
	{
		var doktor = new Doktor { AktifMi = false };

		Assert.Throws<IsKuraliException>(doktor.Pasiflestir);
	}

	[Fact]
	public void Aktiflestir_PasifHesap_AktifOlur()
	{
		var doktor = new Doktor { AktifMi = false };

		doktor.Aktiflestir();

		Assert.True(doktor.AktifMi);
	}

	[Fact]
	public void Aktiflestir_ZatenAktifHesap_HataVerir()
	{
		var doktor = new Doktor();

		Assert.Throws<IsKuraliException>(doktor.Aktiflestir);
	}
}
