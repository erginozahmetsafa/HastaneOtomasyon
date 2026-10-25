using HastaneOtomasyon.API.Extensions;
using HastaneOtomasyon.Data;
using HastaneOtomasyon.Service;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SqlServer")
	?? throw new InvalidOperationException("'SqlServer' bağlantı cümlesi bulunamadı.");

builder.Services.AddDataLayer(connectionString);
builder.Services.AddServiceLayer();
builder.Services.AddApiLayer(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages(); // 401/403/404 gibi gövdesiz yanıtlar da ProblemDetails döner

if (app.Environment.IsDevelopment())
{
	await app.Services.VeritabaniniGuncelleAsync();
	await app.Services.DemoVerileriniEkleAsync();
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Entegrasyon testlerinde WebApplicationFactory<Program> için erişilebilir olmalı.
public partial class Program;
