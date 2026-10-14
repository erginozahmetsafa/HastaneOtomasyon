using System.ComponentModel.DataAnnotations;

namespace HastaneOtomasyon.Core.DTOs;

// ---- Girdi ----

public record DoktorGuncelleDto
{
	[Range(1, int.MaxValue)]
	public int PoliklinikId { get; init; }

	[Required, StringLength(100)]
	public string UzmanlikAlani { get; init; } = string.Empty;

	[StringLength(30)]
	public string? Unvan { get; init; }
}

public record DoktorHizmetAtaDto
{
	[Range(1, int.MaxValue)]
	public int HizmetId { get; init; }

	// Boş bırakılırsa hizmetin standart ücreti geçerlidir.
	[Range(typeof(decimal), "0", "1000000")]
	public decimal? OzelUcret { get; init; }
}

// ---- Çıktı ----

public record DoktorDto(int Id, string Ad, string Soyad, string? Unvan, string UzmanlikAlani, int HastaneId, int PoliklinikId);

public record DoktorDetayDto(
	DoktorDto Doktor,
	string PoliklinikAd,
	IReadOnlyList<DoktorHizmetDto> Hizmetler,
	IReadOnlyList<MusaitlikDto> Musaitlikler);

// Ucret: doktora özel ücret varsa o, yoksa hizmetin standart ücreti.
public record DoktorHizmetDto(int HizmetId, string HizmetAd, int SureDakika, decimal Ucret);

public record MusaitlikDto(int Id, int DoktorId, DayOfWeek Gun, TimeSpan Baslangic, TimeSpan Bitis, bool AktifMi);
