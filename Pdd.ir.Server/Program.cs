using Microsoft.AspNetCore.StaticFiles;
using Pdd.ir.Business.Services;
using Pdd.ir.Data;
using Pdd.ir.Server.Services;
using Pdd.ir.Server.WebSocket;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

// ── Services ──────────────────────────────────────────────
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiKeyAttribute>();
});
builder.Services.AddEndpointsApiExplorer();

// Data
builder.Services.AddSingleton<IDbService, DbService>();

// Business Services
builder.Services.AddScoped<AuthBusinessService>();
builder.Services.AddScoped<ProductBusinessService>();
builder.Services.AddScoped<ContactBusinessService>();
builder.Services.AddScoped<PageBusinessService>();
builder.Services.AddScoped<BlogBusinessService>();
builder.Services.AddScoped<PortfolioBusinessService>();
builder.Services.AddScoped<RoleBusinessService>();
builder.Services.AddScoped<PermissionBusinessService>();

// Server Services
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<CryptoJsService>();
builder.Services.AddSingleton<AesKeyStore>();
builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddScoped<ScriptExecutor>();
builder.Services.AddSingleton<WebSocketHandler>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5000",
                "http://localhost:5001",
                "https://localhost:7001",
                "https://localhost:7125",
                "http://localhost:5183",
                "http://localhost:8080",
                "http://localhost:80",
                "http://pdd.ir",
                "https://pdd.ir")
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// ── Script Engine (Auto-Migration) ───────────────────────
using (var scope = app.Services.CreateScope())
{
    var scriptExecutor = scope.ServiceProvider.GetRequiredService<ScriptExecutor>();
    var scriptsPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "resource");
    await scriptExecutor.ExecutePendingScriptsAsync(scriptsPath);
}

// ── Middleware Pipeline ───────────────────────────────────
app.UseCors();

// Response Encryption (captures API responses and encrypts them)
app.UseMiddleware<ResponseEncryptionMiddleware>();

// WebSocket
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30),
    AllowedOrigins = { "*" }
});

app.MapControllers();

// WebSocket endpoint
app.Map("/ws", async (HttpContext context) =>
{
    var handler = context.RequestServices.GetRequiredService<WebSocketHandler>();
    await handler.HandleAsync(context);
});

// Serve Blazor WASM static files
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
    {
        { ".wasm", "application/wasm" },
        { ".blat", "application/octet-stream" },
        { ".dat", "application/octet-stream" },
        { ".pdb", "application/octet-stream" },
        { ".woff", "font/woff" },
        { ".woff2", "font/woff2" },
    })
});

app.MapFallbackToFile("index.html");

app.Run();
