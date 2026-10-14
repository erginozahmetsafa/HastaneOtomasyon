using HastaneOtomasyon.Core.DTOs;

namespace HastaneOtomasyon.Core.Services;

public interface IRandevuService
{
	// Doktorun müsaitliği ve mevcut randevularına göre seçilen gündeki boş başlangıç saatleri.
	Task<IReadOnlyList<DateTime>> UygunSaatleriGetirAsync(int doktorId, int hizmetId, DateOnly tarih, CancellationToken ct = default);

	// Hasta: kendi adına randevu oluşturur ve kendi randevularını görür.
	Task<RandevuDto> OlusturAsync(RandevuOlusturDto dto, CancellationToken ct = default);
	Task<IReadOnlyList<RandevuDto>> GetBenimRandevularimAsync(CancellationToken ct = default);

	// Randevunun hastası, doktoru veya hastane yöneticisi
	Task<RandevuDto> GetDetayAsync(int id, CancellationToken ct = default);
	Task IptalEtAsync(int id, string? neden, CancellationToken ct = default);

	// Randevunun doktoru veya hastane yöneticisi (onay mekanizması)
	Task OnaylaAsync(int id, CancellationToken ct = default);
	Task ReddetAsync(int id, string neden, CancellationToken ct = default);
	Task TamamlaAsync(int id, CancellationToken ct = default);
	Task<IReadOnlyList<RandevuDto>> GetDoktorTakvimiAsync(int doktorId, DateOnly baslangic, DateOnly bitis, CancellationToken ct = default);

	// Hastane yöneticisi
	Task<IReadOnlyList<RandevuDto>> GetOnayBekleyenlerAsync(int hastaneId, CancellationToken ct = default);
}
