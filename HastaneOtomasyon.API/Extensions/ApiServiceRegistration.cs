using System.Reflection;
using System.Text.Json.Serialization;
using HastaneOtomasyon.API.ErrorHandling;
using HastaneOtomasyon.API.Security;
using HastaneOtomasyon.Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace HastaneOtomasyon.API.Extensions;

public static class ApiServiceRegistration
{
	public static IServiceCollection AddApiLayer(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddHttpContextAccessor();
		services.AddScoped<IAktifKullanici, AktifKullanici>();
		services.AddSingleton<ITokenUretici, JwtTokenUretici>();

		services.AddRouting(o => o.LowercaseUrls = true);
		services.AddControllers()
			.AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

		services.AddProblemDetails();
		services.AddExceptionHandler<GlobalExceptionHandler>();

		services.AddJwtKimlikDogrulama(configuration);
		services.AddSwaggerDokumantasyonu();

		return services;
	}

	private static void AddJwtKimlikDogrulama(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddOptions<JwtAyarlari>()
			.Bind(configuration.GetSection(JwtAyarlari.Bolum))
			.ValidateDataAnnotations()
			.ValidateOnStart();

		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

		// Doğrulama parametreleri, doğrulanmış JwtAyarlari üzerinden kurulur.
		services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
			.Configure<IOptions<JwtAyarlari>>((options, ayarlar) =>
			{
				var jwt = ayarlar.Value;
				options.MapInboundClaims = false; // claim adları token'daki gibi kalsın (sub, role, ...)
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidIssuer = jwt.Issuer,
					ValidateAudience = true,
					ValidAudience = jwt.Audience,
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = jwt.ImzaAnahtari(),
					ValidateLifetime = true,
					ClockSkew = TimeSpan.FromMinutes(1),
					NameClaimType = ClaimTipleri.AdSoyad,
					RoleClaimType = ClaimTipleri.Rol
				};
			});

		// Varsayılan olarak her uç nokta giriş ister; herkese açık olanlar [AllowAnonymous] ile işaretlenir.
		services.AddAuthorizationBuilder()
			.SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
	}

	private static void AddSwaggerDokumantasyonu(this IServiceCollection services)
	{
		const string semaAdi = "Bearer";

		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(o =>
		{
			o.SwaggerDoc("v1", new OpenApiInfo
			{
				Title = "Hastane Otomasyon API",
				Version = "v1",
				Description = "Çok şubeli hastane/klinik randevu sistemi. " +
					"Önce /api/hesap/giris ile token alın, ardından sağ üstteki Authorize düğmesine yapıştırın."
			});

			o.AddSecurityDefinition(semaAdi, new OpenApiSecurityScheme
			{
				Type = SecuritySchemeType.Http,
				Scheme = "bearer",
				BearerFormat = "JWT",
				In = ParameterLocation.Header,
				Description = "Giriş yanıtındaki token değeri (\"Bearer\" yazmadan)."
			});
			o.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				[new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = semaAdi } }] = []
			});

			// Saat alanları JSON'da "09:30:00" biçimindedir.
			o.MapType<TimeSpan>(() => new OpenApiSchema { Type = "string", Format = "time", Example = new OpenApiString("09:00:00") });

			var xmlDosyasi = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
			o.IncludeXmlComments(xmlDosyasi);
		});
	}
}
