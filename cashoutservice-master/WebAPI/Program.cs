using AutoMapper;
using Core.Application.Interfaces.Bank;
using Core.Model.DTO.Configuration;
using Infrastructure.Libraries.Banks;
using Microsoft.AspNetCore.ResponseCompression;
using Newtonsoft.Json.Linq;
using StackExchange.Redis.Extensions.Core.Configuration;

var builder = WebApplication.CreateBuilder(args);
var configBuilder = new ConfigurationBuilder()
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
string fileName = string.Concat("appsettings.", builder.Environment.EnvironmentName, ".json");
configBuilder.AddJsonFile(fileName, optional: true);
var Configuration = configBuilder.Build();
var appSettingsSection = Configuration.GetSection("SystemVariables");
builder.Services.Configure<SystemVariables>(appSettingsSection);
var appSettings = appSettingsSection.Get<SystemVariables>();
// Add services to the container.

builder.Services.AddCors(options => {
    options.AddPolicy("CorsPolicy",
    builder => builder.AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMvc().AddNewtonsoftJson();
builder.Services.AddSingleton<IEMVCardPayment, InterswitchKimono>()
    .AddSingleton<IEMVCardPayment, Fidesic>()
    .AddSingleton<IEMVCardPayment, LuxByGrupp>()
    .AddSingleton<IEMVCardPayment, ArcaImpl>();
builder.Services.AddResponseCompression(Options => {
    Options.EnableForHttps = true;
    Options.Providers.Add<GzipCompressionProvider>();
});
var t = appSettingsSection.GetSection("Redis").Get<RedisConfiguration>();
builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = $"{t.Hosts[0].Host}:{t.Hosts[0].Port},password={t.Password},ssl={t.Ssl},abortConnect={t.AbortOnConnectFail}";
});
var cnfg = new MapperConfiguration(cfg => {
    cfg.ShouldMapProperty = p => p.GetMethod.IsPublic || p.GetMethod.IsAssembly;
    cfg.CreateMap<JValue, object>().ConvertUsing(source => source.Value);
});
var mapper = cnfg.CreateMapper();
builder.Services.AddSingleton(mapper);
Infrastructure.DI.Resolve.resolve(builder.Services);
Core.Application.DI.Resolve.resolve(builder.Services);
builder.Services.AddControllers();
var app = builder.Build();
app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseWebSockets();
app.UseCors(builder =>
                builder.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
              );
app.Run();
