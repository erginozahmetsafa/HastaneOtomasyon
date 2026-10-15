using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace HastaneOtomasyon.API.Security;

// appsettings içindeki "Jwt" bölümü. Uygulama açılışında doğrulanır; eksik/kısa anahtarla uygulama başlamaz.
public sealed class JwtAyarlari
{
	public const string Bolum = "Jwt";

	[Required]
	public string Issuer { get; set; } = string.Empty;

	[Required]
	public string Audience { get; set; } = string.Empty;

	// HMAC-SHA256 için en az 256 bit (32 karakter). Kaynak koda değil, ortam değişkenine/user-secrets'a konmalıdır.
	[Required, MinLength(32, ErrorMessage = "JWT anahtarı en az 32 karakter olmalıdır.")]
	public string Anahtar { get; set; } = string.Empty;

	[Range(5, 1440)]
	public int SureDakika { get; set; } = 60;

	public SymmetricSecurityKey ImzaAnahtari() => new(Encoding.UTF8.GetBytes(Anahtar));
}
