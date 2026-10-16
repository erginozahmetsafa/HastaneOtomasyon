using HastaneOtomasyon.API.Models;
using HastaneOtomasyon.Core.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace HastaneOtomasyon.API.Security;

public interface ITokenUretici
{
	GirisYanitiDto Uret(OturumBilgisi oturum);
}

public sealed class JwtTokenUretici(IOptions<JwtAyarlari> ayarlar) : ITokenUretici
{
	private readonly JsonWebTokenHandler handler = new();

	public GirisYanitiDto Uret(OturumBilgisi oturum)
	{
		var jwt = ayarlar.Value;
		var kullanici = oturum.Kullanici;
		var simdi = DateTime.UtcNow;
		var bitis = simdi.AddMinutes(jwt.SureDakika);

		var claimler = new Dictionary<string, object>
		{
			[ClaimTipleri.Id] = kullanici.Id.ToString(),
			[ClaimTipleri.Rol] = kullanici.Rol.ToString(),
			[ClaimTipleri.AdSoyad] = $"{kullanici.Ad} {kullanici.Soyad}",
			[ClaimTipleri.Email] = kullanici.Email,
			[ClaimTipleri.GuvenlikDamgasi] = oturum.GuvenlikDamgasi.ToString()
		};
		if (kullanici.HastaneId is int hastaneId)
			claimler[ClaimTipleri.HastaneId] = hastaneId.ToString();

		var token = handler.CreateToken(new SecurityTokenDescriptor
		{
			Issuer = jwt.Issuer,
			Audience = jwt.Audience,
			Claims = claimler,
			IssuedAt = simdi,
			NotBefore = simdi,
			Expires = bitis,
			SigningCredentials = new SigningCredentials(jwt.ImzaAnahtari(), SecurityAlgorithms.HmacSha256)
		});

		return new GirisYanitiDto(token, bitis, kullanici);
	}
}
