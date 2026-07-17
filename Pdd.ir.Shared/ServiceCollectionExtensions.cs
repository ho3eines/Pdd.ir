using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pdd.ir.Shared.Models;
using Pdd.ir.Shared.Services;

namespace Pdd.ir.Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPddSharedServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSettings>(configuration.GetSection("Pdd"));

        services.AddSingleton<IAppStateService, AppStateService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IClientStorageService, ClientStorageService>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IModalService, ModalService>();

        return services;
    }

    public static IServiceCollection AddPddSharedServices(this IServiceCollection services)
    {
        services.AddSingleton<IAppStateService, AppStateService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IClientStorageService, ClientStorageService>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IModalService, ModalService>();

        return services;
    }
}
