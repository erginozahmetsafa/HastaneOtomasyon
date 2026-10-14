namespace HastaneOtomasyon.Core.Enums;

public enum RandevuDurum
{
	Bekliyor = 1,    // Hasta oluşturdu, onay bekleniyor
	Onaylandi = 2,   // Doktor/yönetici onayladı
	Reddedildi = 3,  // Doktor/yönetici reddetti
	IptalEdildi = 4, // Hasta veya kurum iptal etti
	Tamamlandi = 5   // Muayene/işlem gerçekleşti
}
