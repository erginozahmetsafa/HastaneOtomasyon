using HastaneOtomasyon.Core.Enums;

namespace HastaneOtomasyon.Core.Entities;

// Bağlı olduğu hastanenin bölüm, hizmet, personel ve randevularını yönetir.
public class Yonetici : Calisan
{
	public override Roller Rol => Roller.Yonetici;
}
