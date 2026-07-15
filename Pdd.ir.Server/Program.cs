using Pdd.ir.Business.Services;
using Pdd.ir.Data;
using Pdd.ir.Server.Services;
using Pdd.ir.Server.WebSocket;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────
builder.Services.AddControllers();
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

// WebSocket
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30),
    AllowedOrigins = { "http://localhost:5000", "http://localhost:5001", "https://localhost:7001", "http://localhost:5183", "https://localhost:5183", "https://localhost:7125", "http://localhost:7125" }
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
app.UseStaticFiles();

app.MapFallbackToFile("index.html");

app.Run();
