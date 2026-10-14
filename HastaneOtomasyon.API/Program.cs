using HastaneOtomasyon.API.Security;
using HastaneOtomasyon.Core.Services;
using HastaneOtomasyon.Data;
using HastaneOtomasyon.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("SqlServer")
	?? throw new InvalidOperationException("'SqlServer' bağlantı cümlesi bulunamadı.");
builder.Services.AddDataLayer(connectionString);
builder.Services.AddServiceLayer();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAktifKullanici, AktifKullanici>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
