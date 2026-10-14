using HastaneOtomasyon.Core.DTOs;

namespace HastaneOtomasyon.Core.Services;

public interface IHesapService
{
	// Herkese açık; hastalar kendileri kayıt olur.
	Task<KullaniciDto> HastaKayitAsync(HastaKayitDto dto, CancellationToken ct = default);

	// Bilgiler hatalıysa null döner.
	Task<KullaniciDto?> GirisAsync(GirisDto dto, CancellationToken ct = default);
}
