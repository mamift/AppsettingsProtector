using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppsettingsProtector.Extensions;

public static class ServiceExtensions
{
    /// <summary>
    /// Registers the <see cref="PersistentEncryptor"/> to the service collection, using the <see cref="IEncryptor"/> interface.
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="startupEncryptor"></param>
    /// <param name="purpose">The DPAPI provides a separate purpose as a string; which allows creating multiple providers that can separately encrypt/decrypt their own categories of files.</param>
    /// <param name="withDpApi">Also invoke the <see cref="DataProtectionServiceCollectionExtensions.AddDataProtectionServices"/> extension method. Set this to false if you need to invoke it earlier than there.</param>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IServiceCollection AddPersistentEncryptor(this IServiceCollection collection, out IEncryptor startupEncryptor, 
        string purpose = "PersistentEncryption", bool withDpApi = true, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        if (purpose == null) throw new ArgumentNullException(nameof(purpose));
        if (withDpApi) collection.AddDataProtection();

        switch (lifetime) {
            case ServiceLifetime.Scoped:
                collection.AddScoped<IEncryptor, PersistentEncryptor>(PersistentEncryptorFactory);
                break;

            case ServiceLifetime.Singleton:
                collection.AddSingleton<IEncryptor, PersistentEncryptor>(PersistentEncryptorFactory);
                break;

            default:
                collection.AddTransient<IEncryptor, PersistentEncryptor>(PersistentEncryptorFactory);
                break;
        }

        // returns a single instance that can be used for startup logic
        var sp = collection.BuildServiceProvider();
        startupEncryptor = sp.GetRequiredService<IEncryptor>();

        return collection;

        PersistentEncryptor PersistentEncryptorFactory(IServiceProvider provider)
        {
            var dataProvider = provider.GetRequiredService<IDataProtectionProvider>();
            var protector = dataProvider.CreatePersistedDataProtector(purpose);
            return new PersistentEncryptor(protector);
        }
    }

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