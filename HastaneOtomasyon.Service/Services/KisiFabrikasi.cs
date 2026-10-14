using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Entities;
using HastaneOtomasyon.Core.Exceptions;
using HastaneOtomasyon.Core.Repositories;
using HastaneOtomasyon.Core.Services;

namespace HastaneOtomasyon.Service.Services;

// Hasta ve personel kayıtlarında ortak adımlar: tekillik kontrolü, ortak alanlar, şifre hash'i.
internal sealed class KisiFabrikasi(IKisiRepository kisiRepository, ISifreHasher sifreHasher)
{
	public async Task<T> OlusturAsync<T>(KisiKayitDto dto, CancellationToken ct) where T : Kisi, new()
	{
		var email = EmailNormalizeEt(dto.Email);

		if (await kisiRepository.AnyAsync(k => k.Email == email || k.TcKimlikNo == dto.TcKimlikNo, ct))
			throw new IsKuraliException("Bu e-posta adresi veya TC kimlik numarası ile kayıtlı bir kullanıcı zaten var.");

		return new T
		{
			TcKimlikNo = dto.TcKimlikNo,
			Ad = dto.Ad.Trim(),
			Soyad = dto.Soyad.Trim(),
			Email = email,
			Telefon = dto.Telefon?.Trim(),
			SifreHash = sifreHasher.Hashle(dto.Sifre)
		};
	}

	public static string EmailNormalizeEt(string email) => email.Trim().ToLowerInvariant();
}
