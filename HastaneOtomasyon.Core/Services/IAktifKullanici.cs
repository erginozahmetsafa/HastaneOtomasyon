using HastaneOtomasyon.Core.Enums;

namespace HastaneOtomasyon.Core.Services;

// İsteği yapan kullanıcı. Gerçekleştirimi sunum katmanındadır (API'de token'dan okunur).
public interface IAktifKullanici
{
	int? Id { get; }
	Roller? Rol { get; }
	int? HastaneId { get; } // Yalnızca çalışanlar için dolu
}
