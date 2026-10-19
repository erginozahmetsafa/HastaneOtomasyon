using HastaneOtomasyon.Core.Enums;
using HastaneOtomasyon.Core.Services;

namespace HastaneOtomasyon.UnitTests.Yardimcilar;

// Testlerde isteği yapan kullanıcı. API'de bu bilgi token'dan okunur.
public sealed record TestKullanici(int? Id, Roller? Rol, int? HastaneId = null) : IAktifKullanici
{
	public static readonly TestKullanici Anonim = new(null, null);

	public static TestKullanici HastaOlarak(int id) => new(id, Roller.Hasta);
	public static TestKullanici DoktorOlarak(int id, int hastaneId) => new(id, Roller.Doktor, hastaneId);
	public static TestKullanici PersonelOlarak(int id, int hastaneId) => new(id, Roller.SaglikPersoneli, hastaneId);
	public static TestKullanici YoneticiOlarak(int id, int hastaneId) => new(id, Roller.Yonetici, hastaneId);
	public static TestKullanici SistemYoneticisiOlarak(int id = 1) => new(id, Roller.SistemYoneticisi);
}
