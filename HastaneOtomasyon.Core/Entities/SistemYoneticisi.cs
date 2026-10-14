using HastaneOtomasyon.Core.Enums;

namespace HastaneOtomasyon.Core.Entities;

// Bir hastaneye bağlı değildir; hastaneleri ve hastane yöneticilerini tanımlar.
public class SistemYoneticisi : Kisi
{
	public override Roller Rol => Roller.SistemYoneticisi;
}
