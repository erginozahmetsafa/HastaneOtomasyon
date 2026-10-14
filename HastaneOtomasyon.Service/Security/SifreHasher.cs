using System.Security.Cryptography;
using HastaneOtomasyon.Core.Services;

namespace HastaneOtomasyon.Service.Security;

// PBKDF2-SHA256. Saklama biçimi: "{iterasyon}.{tuz(base64)}.{hash(base64)}"
internal sealed class SifreHasher : ISifreHasher
{
	private const int TuzBoyutu = 16;
	private const int HashBoyutu = 32;
	private const int Iterasyon = 100_000;
	private static readonly HashAlgorithmName Algoritma = HashAlgorithmName.SHA256;

	public string Hashle(string sifre)
	{
		var tuz = RandomNumberGenerator.GetBytes(TuzBoyutu);
		var hash = Rfc2898DeriveBytes.Pbkdf2(sifre, tuz, Iterasyon, Algoritma, HashBoyutu);
		return $"{Iterasyon}.{Convert.ToBase64String(tuz)}.{Convert.ToBase64String(hash)}";
	}

	public bool Dogrula(string sifre, string sifreHash)
	{
		var parcalar = sifreHash.Split('.');
		if (parcalar.Length != 3 || !int.TryParse(parcalar[0], out var iterasyon))
			return false;

		var tuz = Convert.FromBase64String(parcalar[1]);
		var beklenen = Convert.FromBase64String(parcalar[2]);
		var hash = Rfc2898DeriveBytes.Pbkdf2(sifre, tuz, iterasyon, Algoritma, beklenen.Length);

		// Zamanlama saldırılarına karşı sabit süreli karşılaştırma
		return CryptographicOperations.FixedTimeEquals(hash, beklenen);
	}
}
