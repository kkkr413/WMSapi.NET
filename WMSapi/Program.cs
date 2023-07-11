

using SampleWebApiAspNetCore.Helpers;
using WMSapi.Repositories;
using WMSapi.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCustomCors("AllowAllOrigins");

//DI
// services.AddSingleton<DataContext>();
services.AddScoped<WMSRepositories_R, WMSRepositories>();
services.AddScoped<WMSservice_R, WMSservice>();
services.AddScoped<LOGSETservice_R, LOGSETservice>();
services.AddScoped<IORepositories_R, IORepositories>();
services.AddScoped<IOService_R, IOService>();

services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

 // .WithOrigins("http://192.168.50.56:3000", "http://localhost:3000")

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("AllowAllOrigins");

app.MapControllers();

app.Run();
