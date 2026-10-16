using HastaneOtomasyon.Core.DTOs;

namespace HastaneOtomasyon.Core.Services;

public interface IHesapService
{
	// Herkese açık; hastalar kendileri kayıt olur.
	Task<OturumBilgisi> HastaKayitAsync(HastaKayitDto dto, CancellationToken ct = default);

	// Bilgiler hatalıysa null döner. Hesap pasifse YetkisizIslemException fırlatılır.
	Task<OturumBilgisi?> GirisAsync(GirisDto dto, CancellationToken ct = default);

	// Giriş yapmış kullanıcı kendi şifresini değiştirir. Diğer oturumları kapanır; yeni oturum bilgisi döner.
	Task<OturumBilgisi> SifreDegistirAsync(SifreDegistirDto dto, CancellationToken ct = default);

	// Token hâlâ geçerli mi? (hesap aktif ve şifre değişmemiş)
	Task<bool> OturumGecerliMiAsync(int kisiId, Guid guvenlikDamgasi, CancellationToken ct = default);
}
