using System.Globalization;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using HastaneOtomasyon.API.ErrorHandling;
using HastaneOtomasyon.API.Security;
using HastaneOtomasyon.Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
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
		services.AddHizSinirlama(configuration);
		services.AddCorsPolitikasi(configuration);
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
				options.Events = new JwtBearerEvents { OnTokenValidated = OturumDogrulama.TokenDogrulandiAsync };
			});

		// Varsayılan olarak her uç nokta giriş ister; herkese açık olanlar [AllowAnonymous] ile işaretlenir.
		services.AddAuthorizationBuilder()
			.SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
	}

	private static void AddHizSinirlama(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddOptions<HizSinirlamaAyarlari>()
			.Bind(configuration.GetSection(HizSinirlamaAyarlari.Bolum))
			.ValidateDataAnnotations()
			.ValidateOnStart();

		services.AddRateLimiter(o =>
		{
			o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

			// Sabit pencere, istemci IP'si başına ayrı sayaç.
			o.AddPolicy(HizSinirlamaPolitikalari.HesapIslemleri, httpContext =>
			{
				var ayarlar = httpContext.RequestServices.GetRequiredService<IOptions<HizSinirlamaAyarlari>>().Value;
				return RateLimitPartition.GetFixedWindowLimiter(
					httpContext.Connection.RemoteIpAddress?.ToString() ?? "bilinmiyor",
					_ => new FixedWindowRateLimiterOptions
					{
						PermitLimit = ayarlar.IzinSayisi,
						Window = TimeSpan.FromSeconds(ayarlar.PencereSaniye),
						QueueLimit = 0
					});
			});

			o.OnRejected = async (context, ct) =>
			{
				if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var bekleme))
					context.HttpContext.Response.Headers.RetryAfter =
						((int)Math.Ceiling(bekleme.TotalSeconds)).ToString(CultureInfo.InvariantCulture);

				await context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>().WriteAsync(new ProblemDetailsContext
				{
					HttpContext = context.HttpContext,
					ProblemDetails =
					{
						Status = StatusCodes.Status429TooManyRequests,
						Title = "Çok fazla deneme",
						Detail = "Kısa sürede çok fazla istek yapıldı. Lütfen biraz bekleyip tekrar deneyin."
					}
				});
			};
		});
	}

	// Tarayıcıdaki bir arayüzün API'yi çağırabilmesi için izin verilen adresler appsettings'ten okunur.
	private static void AddCorsPolitikasi(this IServiceCollection services, IConfiguration configuration)
	{
		var izinliKaynaklar = configuration.GetSection("Cors:IzinliKaynaklar").Get<string[]>() ?? [];

		services.AddCors(o => o.AddDefaultPolicy(policy => policy
			.WithOrigins(izinliKaynaklar)
			.AllowAnyHeader()
			.AllowAnyMethod()
			.WithExposedHeaders("Location", "Retry-After")));
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
