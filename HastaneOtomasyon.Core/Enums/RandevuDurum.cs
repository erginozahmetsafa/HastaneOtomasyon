namespace HastaneOtomasyon.Core.Enums
{
	public enum RandevuDurum
	{
		Bekliyor,    // Hasta oluşturdu, onay bekleniyor
		Onaylandi,   // Doktor/yönetici onayladı
		Reddedildi,  // Doktor/yönetici reddetti
		IptalEdildi, // Hasta veya kurum iptal etti
		Tamamlandi   // Muayene/işlem gerçekleşti
	}
}
