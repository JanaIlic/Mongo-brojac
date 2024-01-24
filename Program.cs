global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using Microsoft.EntityFrameworkCore;
global using System.Text.Json.Serialization;
global using API.Model.Poveznici;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.IdentityModel.Tokens;
global using System.Text;
global using Microsoft.OpenApi.Models;
global using Swashbuckle.AspNetCore.Filters;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.IdentityModel.Tokens.Jwt;
global using System.IO;
global using System.Security;
global using System.Security.AccessControl;
global using System.Security.Principal;

global using MongoDB.Bson;
global using MongoDB.Bson.Serialization.Attributes;
global using MongoDB.Driver;
global using MongoDB.Bson.Serialization;

global using System.Text.Json;

global using Serilog;

global using API.Data;
global using API.Servisi;
global using API.Model;
global using API.Servisi.Interfejsi;
using MongoDB.Driver.Core.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(x => {
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header, 
        Name = "Authorization",
        Type =  SecuritySchemeType.ApiKey
    }); 

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});



builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddCors(options => options.AddPolicy(name: "Brojac", policy => {
    policy.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
}));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
        ValidateIssuer = false,
        ValidateAudience = false
    };

});


builder.Services.AddScoped<IMongoDB, MongoDbServis>();

builder.Services.AddScoped<INalog, NalogServis>();
builder.Services.AddScoped<IAktivnost, AktivnostServis>();
builder.Services.AddScoped<INamirnica, NamirnicaServis>();
builder.Services.AddScoped<IDan, DanServis>();
builder.Services.AddScoped<IJelo, JeloServis>();
builder.Services.AddScoped<IObjava, ObjavaServis>();
builder.Services.AddScoped<IObrok, ObrokServis>();
builder.Services.AddScoped<IPoruka, PorukaServis>();
builder.Services.AddScoped<IStanje, StanjeServis>();
builder.Services.AddScoped<ITrening, TreningServis>();
builder.Services.AddScoped<IZahtevAktivnosti, ZahtevAktivnostiServis>();
builder.Services.AddScoped<IZahtevNamirnice, ZahtevNamirniceServis>();
builder.Services.AddScoped<IZahtevZaPracenje, ZahtevZaPracenjeServis>();



builder.Services.AddHttpContextAccessor();

Log.Logger = new LoggerConfiguration().MinimumLevel.Information().
    WriteTo.File("logs/NoSqlLog.txt", rollingInterval: RollingInterval.Infinite).CreateLogger();



WebApplication app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors("Brojac");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
