using System.ComponentModel.DataAnnotations;

namespace HastaneOtomasyon.Core.DTOs;

// ---- Girdi ----

public record HastaneKaydetDto
{
	[Required, StringLength(150)]
	public string Ad { get; init; } = string.Empty;

	[Required, StringLength(500)]
	public string Adres { get; init; } = string.Empty;

	[Phone, StringLength(20)]
	public string? Telefon { get; init; }
}

public record PoliklinikOlusturDto
{
	[Range(1, int.MaxValue)]
	public int HastaneId { get; init; }

	[Required, StringLength(100)]
	public string Ad { get; init; } = string.Empty;

	[StringLength(500)]
	public string? Aciklama { get; init; }
}

public record PoliklinikGuncelleDto
{
	[Required, StringLength(100)]
	public string Ad { get; init; } = string.Empty;

	[StringLength(500)]
	public string? Aciklama { get; init; }
}

// Çalışma saati ve müsaitlik tanımlarında ortak kullanılır.
public record ZamanAraligiDto
{
	public DayOfWeek Gun { get; init; }
	public TimeSpan Baslangic { get; init; }
	public TimeSpan Bitis { get; init; }
}

// ---- Çıktı ----

public record HastaneDto(int Id, string Ad, string Adres, string? Telefon);

public record HastaneDetayDto(
	int Id,
	string Ad,
	string Adres,
	string? Telefon,
	IReadOnlyList<PoliklinikDto> Poliklinikler,
	IReadOnlyList<CalismaSaatiDto> CalismaSaatleri);

public record PoliklinikDto(int Id, int HastaneId, string Ad, string? Aciklama);

public record CalismaSaatiDto(int Id, DayOfWeek Gun, TimeSpan Baslangic, TimeSpan Bitis);
