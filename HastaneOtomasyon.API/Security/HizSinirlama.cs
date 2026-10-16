using System.ComponentModel.DataAnnotations;

namespace HastaneOtomasyon.API.Security;

public static class HizSinirlamaPolitikalari
{
	// Giriş, kayıt ve şifre değiştirme: şifre deneme saldırılarına karşı IP başına sınırlanır.
	public const string HesapIslemleri = "hesap-islemleri";
}

// appsettings içindeki "HizSinirlama" bölümü.
public sealed class HizSinirlamaAyarlari
{
	public const string Bolum = "HizSinirlama";

	[Range(1, 10_000)]
	public int IzinSayisi { get; set; } = 10;

	[Range(1, 3_600)]
	public int PencereSaniye { get; set; } = 60;
}
