using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Pdd.ir.Client;
using Pdd.ir.Client.Services;
using Pdd.ir.Client.Models;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("Pdd"));

// ── HttpClient with API Key header ──
builder.Services.AddScoped(sp =>
{
    var config = builder.Configuration;
    var apiKey = config["Pdd:ApiSettings:APIKey"] ?? "";

    var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
    if (!string.IsNullOrEmpty(apiKey))
        http.DefaultRequestHeaders.Add("X-API-Key", apiKey);

    return http;
});

builder.Services.AddScoped<EncryptionService>();
builder.Services.AddScoped<ICommunicationService, CommunicationService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ConnectionService>();
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<AnimationService>();
builder.Services.AddScoped<ITranslateService, TranslateService>();

// Shared services (formerly Pdd.ir.Shared)
builder.Services.AddSingleton<IAppStateService, AppStateService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IClientStorageService, ClientStorageService>();
builder.Services.AddScoped<IEncryptionService, PddEncryptionService>();
builder.Services.AddScoped<IModalService, ModalService>();

var app = builder.Build();

var translateService = app.Services.GetRequiredService<ITranslateService>();
await translateService.InitializeAsync();

await app.RunAsync();
