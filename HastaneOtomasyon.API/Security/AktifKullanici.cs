using System.Security.Claims;
using HastaneOtomasyon.Core.Enums;
using HastaneOtomasyon.Core.Services;

namespace HastaneOtomasyon.API.Security;

// İsteği yapan kullanıcıyı token claim'lerinden okur. Giriş yapılmamışsa tüm değerler null'dır.
public class AktifKullanici(IHttpContextAccessor httpContextAccessor) : IAktifKullanici
{
	public const string HastaneIdClaim = "hastane_id";

	private ClaimsPrincipal? Kullanici => httpContextAccessor.HttpContext?.User;

	public int? Id => SayiClaim(ClaimTypes.NameIdentifier);

	public Roller? Rol =>
		Enum.TryParse<Roller>(Kullanici?.FindFirstValue(ClaimTypes.Role), out var rol) ? rol : null;

	public int? HastaneId => SayiClaim(HastaneIdClaim);

	private int? SayiClaim(string tip) =>
		int.TryParse(Kullanici?.FindFirstValue(tip), out var deger) ? deger : null;
}
