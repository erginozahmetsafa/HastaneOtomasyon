using Microsoft.IdentityModel.JsonWebTokens;

namespace HastaneOtomasyon.API.Security;

// Token'a yazılan ve okunan claim adları. Tek yerde tutulur ki üretim ve okuma tutarlı kalsın.
public static class ClaimTipleri
{
	public const string Id = JwtRegisteredClaimNames.Sub;
	public const string Rol = "role";
	public const string AdSoyad = JwtRegisteredClaimNames.Name;
	public const string Email = JwtRegisteredClaimNames.Email;
	public const string HastaneId = "hastane_id";
}
