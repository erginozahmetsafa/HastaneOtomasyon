namespace HastaneOtomasyon.Core.Enums;

public enum Roller
{
	Hasta = 1,
	Doktor = 2,
	SaglikPersoneli = 3,
	Yonetici = 4,         // Tek bir hastanenin yöneticisi
	SistemYoneticisi = 5  // Tüm şubeleri yönetir; hastane ve hastane yöneticisi tanımlar
}
