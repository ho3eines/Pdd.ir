using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Pdd.ir.Client;
using Pdd.ir.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5183/") });
builder.Services.AddScoped<EncryptionService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ConnectionService>();

await builder.Build().RunAsync();
