using System.Security.Claims;
using HastaneOtomasyon.Core.Enums;
using HastaneOtomasyon.Core.Services;

namespace HastaneOtomasyon.API.Security;

// İsteği yapan kullanıcıyı token claim'lerinden okur. Giriş yapılmamışsa tüm değerler null'dır.
public class AktifKullanici(IHttpContextAccessor httpContextAccessor) : IAktifKullanici
{
	private ClaimsPrincipal? Kullanici => httpContextAccessor.HttpContext?.User;

	public int? Id => SayiClaim(ClaimTipleri.Id);

	public Roller? Rol =>
		Enum.TryParse<Roller>(Kullanici?.FindFirstValue(ClaimTipleri.Rol), out var rol) ? rol : null;

	public int? HastaneId => SayiClaim(ClaimTipleri.HastaneId);

	private int? SayiClaim(string tip) =>
		int.TryParse(Kullanici?.FindFirstValue(tip), out var deger) ? deger : null;
}
