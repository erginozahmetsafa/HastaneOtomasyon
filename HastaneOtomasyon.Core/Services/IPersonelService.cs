using HastaneOtomasyon.Core.DTOs;

namespace HastaneOtomasyon.Core.Services;

public interface IPersonelService
{
	// Herkese açık (randevu alırken doktor seçimi için)
	Task<IReadOnlyList<DoktorDto>> GetDoktorlarAsync(int poliklinikId, CancellationToken ct = default);
	Task<IReadOnlyList<DoktorDto>> GetDoktorlarByHizmetAsync(int hizmetId, CancellationToken ct = default);
	Task<DoktorDetayDto> GetDoktorDetayAsync(int doktorId, CancellationToken ct = default);

	// Sistem yöneticisi veya o hastanenin yöneticisi
	Task<IReadOnlyList<PersonelDto>> GetPersonellerAsync(int hastaneId, CancellationToken ct = default);
	Task<PersonelDto> DoktorEkleAsync(DoktorKayitDto dto, CancellationToken ct = default);
	Task<PersonelDto> SaglikPersoneliEkleAsync(SaglikPersoneliKayitDto dto, CancellationToken ct = default);
	Task DoktorGuncelleAsync(int doktorId, DoktorGuncelleDto dto, CancellationToken ct = default);
	Task DoktorHizmetAtaAsync(int doktorId, DoktorHizmetAtaDto dto, CancellationToken ct = default);
	Task DoktorHizmetKaldirAsync(int doktorId, int hizmetId, CancellationToken ct = default);

	// Yalnızca sistem yöneticisi
	Task<PersonelDto> YoneticiEkleAsync(YoneticiKayitDto dto, CancellationToken ct = default);
}
