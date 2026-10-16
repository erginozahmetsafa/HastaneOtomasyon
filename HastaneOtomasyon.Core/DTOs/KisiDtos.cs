using System.ComponentModel.DataAnnotations;
using HastaneOtomasyon.Core.Enums;

namespace HastaneOtomasyon.Core.DTOs;

// Kayıt ve şifre değiştirmede ortak şifre politikası: en az 8 karakter, en az bir harf ve bir rakam.
internal static class SifreKurali
{
	public const string Desen = @"^(?=.*[A-Za-zÇĞİÖŞÜçğıöşü])(?=.*\d).{8,100}$";
	public const string Mesaj = "Şifre en az 8 karakter olmalı ve en az bir harf ile bir rakam içermelidir.";
}

// ---- Girdi ----

public abstract record KisiKayitDto
{
	[Required, RegularExpression("^[0-9]{11}$", ErrorMessage = "TC kimlik numarası 11 haneli olmalıdır.")]
	public string TcKimlikNo { get; init; } = string.Empty;

	[Required, StringLength(50)]
	public string Ad { get; init; } = string.Empty;

	[Required, StringLength(50)]
	public string Soyad { get; init; } = string.Empty;

	[Required, EmailAddress, StringLength(100)]
	public string Email { get; init; } = string.Empty;

	[Phone, StringLength(20)]
	public string? Telefon { get; init; }

	[Required, RegularExpression(SifreKurali.Desen, ErrorMessage = SifreKurali.Mesaj)]
	public string Sifre { get; init; } = string.Empty;
}

public record HastaKayitDto : KisiKayitDto
{
	public DateTime DogumTarihi { get; init; }

	[StringLength(10)]
	public string? Cinsiyet { get; init; }

	[StringLength(10)]
	public string? KanGrubu { get; init; }
}

public abstract record CalisanKayitDto : KisiKayitDto
{
	[Range(1, int.MaxValue)]
	public int HastaneId { get; init; }

	// Boş bırakılırsa bugünün tarihi kullanılır.
	public DateTime? IseBaslamaTarihi { get; init; }
}

public record DoktorKayitDto : CalisanKayitDto
{
	[Range(1, int.MaxValue)]
	public int PoliklinikId { get; init; }

	[Required, StringLength(100)]
	public string UzmanlikAlani { get; init; } = string.Empty;

	[StringLength(30)]
	public string? Unvan { get; init; }
}

public record SaglikPersoneliKayitDto : CalisanKayitDto
{
	[Required, StringLength(50)]
	public string Gorev { get; init; } = string.Empty;

	public int? PoliklinikId { get; init; }
}

public record YoneticiKayitDto : CalisanKayitDto;

public record GirisDto
{
	[Required, EmailAddress]
	public string Email { get; init; } = string.Empty;

	[Required]
	public string Sifre { get; init; } = string.Empty;
}

public record SifreDegistirDto
{
	[Required]
	public string MevcutSifre { get; init; } = string.Empty;

	[Required, RegularExpression(SifreKurali.Desen, ErrorMessage = SifreKurali.Mesaj)]
	public string YeniSifre { get; init; } = string.Empty;
}

// ---- Çıktı ----

public record KullaniciDto(int Id, string Ad, string Soyad, string Email, Roller Rol, int? HastaneId);

// Oturum açan katmanın (API) token üretmesi için gereken bilgi. Güvenlik damgası istemciye ayrıca gösterilmez.
public record OturumBilgisi(KullaniciDto Kullanici, Guid GuvenlikDamgasi);

public record PersonelDto(int Id, string Ad, string Soyad, string Email, string? Telefon, Roller Rol, int HastaneId, bool AktifMi);

public record PasiflestirmeSonucuDto(int IptalEdilenRandevuSayisi);
