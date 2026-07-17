using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Pdd.ir.Client;
using Pdd.ir.Client.Services;
using Pdd.ir.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<EncryptionService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ConnectionService>();
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<AnimationService>();
builder.Services.AddSingleton<ITranslateService, TranslateService>();
builder.Services.AddScoped<ICommunicationService, CommunicationService>();

// Pdd.ir.Shared services
builder.Services.AddPddSharedServices();

var app = builder.Build();

var commService = app.Services.GetRequiredService<ICommunicationService>();
await commService.InitializeAsync();

// Load saved language
var translateService = app.Services.GetRequiredService<ITranslateService>();
await translateService.LoadLanguageAsync("fa");

await app.RunAsync();
