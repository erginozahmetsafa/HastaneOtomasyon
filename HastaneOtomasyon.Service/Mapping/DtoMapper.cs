using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Entities;

namespace HastaneOtomasyon.Service.Mapping;

// Entity → DTO dönüşümleri. Navigation property kullanan dönüşümler ilgili ilişkilerin yüklenmiş olmasını bekler.
internal static class DtoMapper
{
	public static KullaniciDto ToKullaniciDto(this Kisi k) =>
		new(k.Id, k.Ad, k.Soyad, k.Email, k.Rol, (k as Calisan)?.HastaneId);

	public static PersonelDto ToPersonelDto(this Calisan c) =>
		new(c.Id, c.Ad, c.Soyad, c.Email, c.Telefon, c.Rol, c.HastaneId);

	public static HastaneDto ToDto(this Hastane h) =>
		new(h.Id, h.Ad, h.Adres, h.Telefon);

	public static HastaneDetayDto ToDetayDto(this Hastane h) =>
		new(h.Id, h.Ad, h.Adres, h.Telefon,
			h.Poliklinikler.OrderBy(p => p.Ad).Select(ToDto).ToList(),
			h.CalismaSaatleri.OrderBy(c => c.Gun).ThenBy(c => c.Baslangic).Select(ToDto).ToList());

	public static PoliklinikDto ToDto(this Poliklinik p) =>
		new(p.Id, p.HastaneId, p.Ad, p.Aciklama);

	public static CalismaSaatiDto ToDto(this CalismaSaati c) =>
		new(c.Id, c.Gun, c.Baslangic, c.Bitis);

	public static HizmetDto ToDto(this Hizmet h) =>
		new(h.Id, h.PoliklinikId, h.Ad, h.Aciklama, h.SureDakika, h.Ucret, h.AktifMi);

	public static DoktorDto ToDto(this Doktor d) =>
		new(d.Id, d.Ad, d.Soyad, d.Unvan, d.UzmanlikAlani, d.HastaneId, d.PoliklinikId);

	public static DoktorDetayDto ToDetayDto(this Doktor d) =>
		new(d.ToDto(),
			d.Poliklinik.Ad,
			d.DoktorHizmetler.OrderBy(dh => dh.Hizmet.Ad).Select(ToDto).ToList(),
			d.Musaitlikler.OrderBy(m => m.Gun).ThenBy(m => m.Baslangic).Select(ToDto).ToList());

	public static DoktorHizmetDto ToDto(this DoktorHizmet dh) =>
		new(dh.HizmetId, dh.Hizmet.Ad, dh.Hizmet.SureDakika, dh.OzelUcret ?? dh.Hizmet.Ucret);

	public static MusaitlikDto ToDto(this Musaitlik m) =>
		new(m.Id, m.DoktorId, m.Gun, m.Baslangic, m.Bitis, m.AktifMi);

	public static RandevuDto ToDto(this Randevu r) =>
		new(r.Id,
			r.HastaId, $"{r.Hasta.Ad} {r.Hasta.Soyad}",
			r.DoktorId, $"{r.Doktor.Unvan} {r.Doktor.Ad} {r.Doktor.Soyad}".TrimStart(),
			r.Doktor.Poliklinik.Ad,
			r.HizmetId, r.Hizmet.Ad,
			r.Baslangic, r.Bitis, r.SureDakika, r.Ucret,
			r.Durum, r.IptalRedNedeni, r.HastaNotu);
}
