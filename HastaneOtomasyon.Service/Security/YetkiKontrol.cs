using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Enums;
using HastaneOtomasyon.Core.Exceptions;
using HastaneOtomasyon.Core.Services;

namespace HastaneOtomasyon.Service.Security;

// Servislerde tekrar eden yetki kurallarını tek yerde toplar.
internal static class YetkiKontrol
{
	public static int GirisYapmisOlmali(this IAktifKullanici kullanici) =>
		kullanici.Id ?? throw new YetkisizIslemException("Bu işlem için giriş yapmalısınız.");

	// Kullanıcının Id'sini döner.
	public static int RolOlmali(this IAktifKullanici kullanici, Roller rol)
	{
		var id = kullanici.GirisYapmisOlmali();
		if (kullanici.Rol != rol)
			throw new YetkisizIslemException();

		return id;
	}

	public static bool HastaneYoneticisiMi(this IAktifKullanici kullanici, int hastaneId) =>
		kullanici.Rol == Roller.SistemYoneticisi ||
		(kullanici.Rol == Roller.Yonetici && kullanici.HastaneId == hastaneId);

	public static void HastaneYonetebilmeli(this IAktifKullanici kullanici, int hastaneId)
	{
		kullanici.GirisYapmisOlmali();
		if (!kullanici.HastaneYoneticisiMi(hastaneId))
			throw new YetkisizIslemException();
	}

	// Doktorun kendisi veya hastanesinin yöneticisi
	public static void DoktorYonetebilmeli(this IAktifKullanici kullanici, Doktor doktor)
	{
		kullanici.GirisYapmisOlmali();
		var doktorunKendisi = kullanici.Rol == Roller.Doktor && kullanici.Id == doktor.Id;
		if (!doktorunKendisi && !kullanici.HastaneYoneticisiMi(doktor.HastaneId))
			throw new YetkisizIslemException();
	}
}
