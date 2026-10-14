using System.ComponentModel.DataAnnotations;
using HastaneOtomasyon.Core.Enums;

namespace HastaneOtomasyon.Core.DTOs;

// ---- Girdi ----

public record RandevuOlusturDto
{
	[Range(1, int.MaxValue)]
	public int DoktorId { get; init; }

	[Range(1, int.MaxValue)]
	public int HizmetId { get; init; }

	public DateTime Baslangic { get; init; }

	[StringLength(500)]
	public string? HastaNotu { get; init; }
}

// ---- Çıktı ----

public record RandevuDto(
	int Id,
	int HastaId,
	string HastaAdSoyad,
	int DoktorId,
	string DoktorAdSoyad,
	string PoliklinikAd,
	int HizmetId,
	string HizmetAd,
	DateTime Baslangic,
	DateTime Bitis,
	int SureDakika,
	decimal Ucret,
	RandevuDurum Durum,
	string? IptalRedNedeni,
	string? HastaNotu);
