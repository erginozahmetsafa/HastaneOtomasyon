using HastaneOtomasyon.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace HastaneOtomasyon.API.ErrorHandling;

// Servis katmanının fırlattığı hataları standart ProblemDetails (RFC 7807) yanıtlarına çevirir.
// Controller'larda try/catch gerekmez.
// ExceptionHandlerMiddleware'in kendi hata kaydı appsettings'te kapatılmıştır (iş kuralı ihlalleri
// sunucu hatası gibi loglanmasın); beklenmeyen hatalar burada loglanır.
internal sealed class GlobalExceptionHandler(
	IProblemDetailsService problemDetailsService,
	ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
	{
		var girisYapmis = httpContext.User.Identity?.IsAuthenticated == true;

		var (durumKodu, baslik, detay) = exception switch
		{
			BulunamadiException => (StatusCodes.Status404NotFound, "Kayıt bulunamadı", exception.Message),
			IsKuraliException => (StatusCodes.Status400BadRequest, "İş kuralı ihlali", exception.Message),
			YetkisizIslemException when !girisYapmis => (StatusCodes.Status401Unauthorized, "Giriş gerekli", exception.Message),
			YetkisizIslemException => (StatusCodes.Status403Forbidden, "Yetkisiz işlem", exception.Message),
			DbUpdateException => (StatusCodes.Status409Conflict, "Kayıt çakışması", "Kayıt, mevcut verilerle çakıştığı için kaydedilemedi."),
			_ => (StatusCodes.Status500InternalServerError, "Sunucu hatası", "Beklenmeyen bir hata oluştu.")
		};

		if (durumKodu >= StatusCodes.Status500InternalServerError)
			logger.LogError(exception, "Beklenmeyen hata: {Yol}", httpContext.Request.Path);
		else if (durumKodu == StatusCodes.Status409Conflict)
			logger.LogWarning(exception, "Veritabanı çakışması: {Yol}", httpContext.Request.Path);

		httpContext.Response.StatusCode = durumKodu;
		return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
		{
			HttpContext = httpContext,
			Exception = exception,
			ProblemDetails = { Status = durumKodu, Title = baslik, Detail = detay }
		});
	}
}
