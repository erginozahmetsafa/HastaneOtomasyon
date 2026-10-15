using Microsoft.AspNetCore.Mvc;

namespace HastaneOtomasyon.API.Controllers;

// Tüm controller'ların ortak ayarları. İş mantığı servislerdedir; controller'lar yalnızca HTTP'ye çevirir.
// Hata durumları GlobalExceptionHandler tarafından ProblemDetails'e dönüştürülür.
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
	// Kaynağın tekil GET adresi olmadığında kullanılan 201 yanıtı.
	protected ObjectResult Olusturuldu<T>(T deger) => StatusCode(StatusCodes.Status201Created, deger);
}
