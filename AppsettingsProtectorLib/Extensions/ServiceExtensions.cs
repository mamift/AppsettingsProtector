using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppsettingsProtector.Extensions
{
    public static class ServiceExtensions
    {
        public static void GetEncryptedSettings<TSettings>(this IServiceCollection services, IConfiguration configuration, string? filter = null)
            where TSettings : class, new()
        {
            TSettings? settings = string.IsNullOrEmpty(filter)
                ? configuration.Get<TSettings>()
                : configuration.GetSection(filter!).Get<TSettings>();

            if (settings != null) services.AddSingleton(settings);

            throw new ArgumentNullException("Unable to parse instance of " + nameof(TSettings));
        }
        
        public static void AddJsonEncryptedSettings<TSettings>(this IServiceCollection services, IConfiguration configuration, string? filter = null)
            where TSettings : class, new()
        {
            TSettings? settings = string.IsNullOrEmpty(filter)
                ? configuration.Get<TSettings>()
                : configuration.GetSection(filter!).Get<TSettings>();

            if (settings != null) services.AddSingleton(settings);

            throw new ArgumentNullException("Unable to parse instance of " + nameof(TSettings));
        }
    }
}