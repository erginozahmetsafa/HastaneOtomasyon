using HastaneOtomasyon.Core.Enums;
using HastaneOtomasyon.Core.Exceptions;

namespace HastaneOtomasyon.Core.Entities;

public class Randevu : BaseEntity
{
	public int HastaId { get; set; }
	public Hasta Hasta { get; set; } = null!;
	public int DoktorId { get; set; }
	public Doktor Doktor { get; set; } = null!;
	public int HizmetId { get; set; }
	public Hizmet Hizmet { get; set; } = null!;

	// Çakışma kontrolü için başlangıç ve bitiş birlikte tutulur.
	public DateTime Baslangic { get; set; }
	public DateTime Bitis { get; set; }

	// Randevu anındaki süre ve ücret; hizmet sonradan değişse de kayıt korunur.
	public int SureDakika { get; set; }
	public decimal Ucret { get; set; }

	// Onay mekanizması
	public RandevuDurum Durum { get; set; } = RandevuDurum.Bekliyor;
	public DateTime? OnayTarihi { get; set; }
	public int? OnaylayanId { get; set; } // Doktor, Yönetici veya Sistem Yöneticisi
	public Kisi? Onaylayan { get; set; }
	public string? IptalRedNedeni { get; set; }
	public string? HastaNotu { get; set; }

	// Bekleyen ve onaylı randevular takvimde yer kaplar.
	public bool AktifMi => Durum is RandevuDurum.Bekliyor or RandevuDurum.Onaylandi;

	public static Randevu Olustur(int hastaId, int doktorId, Hizmet hizmet, decimal ucret, DateTime baslangic, string? hastaNotu) => new()
	{
		HastaId = hastaId,
		DoktorId = doktorId,
		HizmetId = hizmet.Id,
		Baslangic = baslangic,
		Bitis = baslangic.AddMinutes(hizmet.SureDakika),
		SureDakika = hizmet.SureDakika,
		Ucret = ucret,
		HastaNotu = hastaNotu,
		Durum = RandevuDurum.Bekliyor
	};

	public void Onayla(int onaylayanId, DateTime simdi)
	{
		DurumOlmali(RandevuDurum.Bekliyor, "Yalnızca onay bekleyen randevular onaylanabilir.");
		BaslamamisOlmali(simdi);

		Durum = RandevuDurum.Onaylandi;
		OnaylayanId = onaylayanId;
		OnayTarihi = simdi;
	}

	public void Reddet(int onaylayanId, string neden, DateTime simdi)
	{
		DurumOlmali(RandevuDurum.Bekliyor, "Yalnızca onay bekleyen randevular reddedilebilir.");
		if (string.IsNullOrWhiteSpace(neden))
			throw new IsKuraliException("Ret nedeni belirtilmelidir.");

		Durum = RandevuDurum.Reddedildi;
		OnaylayanId = onaylayanId;
		OnayTarihi = simdi;
		IptalRedNedeni = neden.Trim();
	}

	public void IptalEt(string? neden, DateTime simdi)
	{
		if (!AktifMi)
			throw new IsKuraliException("Yalnızca bekleyen veya onaylı randevular iptal edilebilir.");
		BaslamamisOlmali(simdi);

		Durum = RandevuDurum.IptalEdildi;
		IptalRedNedeni = neden?.Trim();
	}

	public void Tamamla(DateTime simdi)
	{
		DurumOlmali(RandevuDurum.Onaylandi, "Yalnızca onaylı randevular tamamlanabilir.");
		if (Baslangic > simdi)
			throw new IsKuraliException("Henüz başlamamış bir randevu tamamlanamaz.");

		Durum = RandevuDurum.Tamamlandi;
	}

	private void DurumOlmali(RandevuDurum beklenen, string hataMesaji)
	{
		if (Durum != beklenen)
			throw new IsKuraliException(hataMesaji);
	}

	private void BaslamamisOlmali(DateTime simdi)
	{
		if (Baslangic <= simdi)
			throw new IsKuraliException("Başlamış veya geçmiş bir randevu üzerinde bu işlem yapılamaz.");
	}
}
