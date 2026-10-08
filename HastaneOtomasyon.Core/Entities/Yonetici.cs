using HastaneOtomasyon.Core.Enums;

namespace HastaneOtomasyon.Core.Entities
{
	// Bağlı olduğu hastane/klinik üzerinde tanımlama yetkisine sahip kullanıcı.
	public class Yonetici : Calisan
	{
		public override Roller Rol => Roller.Yonetici;
	}
}
