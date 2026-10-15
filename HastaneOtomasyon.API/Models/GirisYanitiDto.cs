using HastaneOtomasyon.Core.DTOs;
using HastaneOtomasyon.Core.Enums;

namespace HastaneOtomasyon.API.Models;

// Kayıt ve giriş yanıtı. Token, sonraki isteklerde "Authorization: Bearer {token}" başlığıyla gönderilir.
public record GirisYanitiDto(string Token, DateTime GecerlilikBitisi, KullaniciDto Kullanici);

public record OturumDto(int Id, Roller Rol, int? HastaneId);
