using System.Security.Cryptography;
using HastaneOtomasyon.Service.Security;

namespace HastaneOtomasyon.UnitTests.Security;

public class SifreHasherTests
{
	private readonly SifreHasher _hasher = new();

	[Fact]
	public void Hashle_IterasyonTuzVeHashBicimindeUretir()
	{
		var parcalar = _hasher.Hashle("Sifre123").Split('.');

		Assert.Equal(3, parcalar.Length);
		Assert.Equal("100000", parcalar[0]);
		Assert.Equal(16, Convert.FromBase64String(parcalar[1]).Length);
		Assert.Equal(32, Convert.FromBase64String(parcalar[2]).Length);
	}

	[Fact]
	public void Hashle_AyniSifre_HerSeferindeFarkliTuzlaFarkliSonucUretir()
	{
		Assert.NotEqual(_hasher.Hashle("Sifre123"), _hasher.Hashle("Sifre123"));
	}

	[Fact]
	public void Hashle_SifreyiAcikMetinOlarakIcermez()
	{
		Assert.DoesNotContain("Sifre123", _hasher.Hashle("Sifre123"));
	}

	[Fact]
	public void Dogrula_DogruSifre_True()
	{
		var hash = _hasher.Hashle("Sifre123");

		Assert.True(_hasher.Dogrula("Sifre123", hash));
	}

	[Theory]
	[InlineData("sifre123")]   // büyük/küçük harf duyarlı
	[InlineData("Sifre1234")]
	[InlineData("")]
	public void Dogrula_YanlisSifre_False(string denenen)
	{
		var hash = _hasher.Hashle("Sifre123");

		Assert.False(_hasher.Dogrula(denenen, hash));
	}

	// Hash'teki iterasyon sayısı okunur; ileride sayı artırılsa da eski hash'ler doğrulanabilir.
	[Fact]
	public void Dogrula_FarkliIterasyonlaUretilmisHash_Dogrulanir()
	{
		var tuz = new byte[16];
		var hash = Rfc2898DeriveBytes.Pbkdf2("Sifre123", tuz, 1_000, HashAlgorithmName.SHA256, 32);
		var eskiBicim = $"1000.{Convert.ToBase64String(tuz)}.{Convert.ToBase64String(hash)}";

		Assert.True(_hasher.Dogrula("Sifre123", eskiBicim));
	}

	[Theory]
	[InlineData("")]
	[InlineData("bozuk")]
	[InlineData("100000.yalnizcaikiparca")]
	[InlineData("sayidegil.AAAA.AAAA")]
	public void Dogrula_BozukBicim_FalseDoner(string bozukHash)
	{
		Assert.False(_hasher.Dogrula("Sifre123", bozukHash));
	}
}
