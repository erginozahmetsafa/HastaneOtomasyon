using HastaneOtomasyon.Core.Enums;

namespace HastaneOtomasyon.API.Security;

// [Authorize(Roles = ...)] için rol listeleri. Ayrıntılı kontrol (hangi hastane, hangi doktor) servis katmanındadır.
public static class RolGruplari
{
	public const string Hasta = nameof(Roller.Hasta);
	public const string SistemYoneticisi = nameof(Roller.SistemYoneticisi);
	public const string Yonetim = SistemYoneticisi + "," + nameof(Roller.Yonetici);
	public const string YonetimVeDoktor = Yonetim + "," + nameof(Roller.Doktor);
}
