using System.ComponentModel.DataAnnotations;

namespace HastaneOtomasyon.Core.DTOs;

// ---- Girdi ----

public record HizmetOlusturDto
{
	[Range(1, int.MaxValue)]
	public int PoliklinikId { get; init; }

	[Required, StringLength(100)]
	public string Ad { get; init; } = string.Empty;

	[StringLength(500)]
	public string? Aciklama { get; init; }

	[Range(5, 480)]
	public int SureDakika { get; init; }

	[Range(typeof(decimal), "0", "1000000")]
	public decimal Ucret { get; init; }
}

public record HizmetGuncelleDto
{
	[Required, StringLength(100)]
	public string Ad { get; init; } = string.Empty;

	[StringLength(500)]
	public string? Aciklama { get; init; }

	[Range(5, 480)]
	public int SureDakika { get; init; }

	[Range(typeof(decimal), "0", "1000000")]
	public decimal Ucret { get; init; }

	public bool AktifMi { get; init; } = true;
}

// ---- Çıktı ----

public record HizmetDto(int Id, int PoliklinikId, string Ad, string? Aciklama, int SureDakika, decimal Ucret, bool AktifMi);
