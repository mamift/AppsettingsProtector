using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppsettingsProtector.Extensions;

public static class ServiceExtensions
{
    /// <summary>
    /// Adds the default <see cref="IPersistedBase64Encryptor"/> encryptor to the service collection and returns an instance that can be used in startup logic.
    /// <para>Use the overload <see cref="AddPersistedEncryptor{TEncryptorInterfaceType,TEncryptorImplType}"/> to add your own implementation.</para>
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="startupEncryptor"></param>
    /// <returns></returns>
    public static IServiceCollection AddPersistedEncryptorWithDefaults(this IServiceCollection collection, out IPersistedBase64Encryptor startupEncryptor)
    {
        if (collection.Any(s => s.ImplementationType == typeof(PersistedBase64Encryptor))) {
            throw new AppsettingsProtectorException("Does not support being called twice");
        }

        var innerInvocation = collection.AddPersistedEncryptor<IPersistedBase64Encryptor, PersistedBase64Encryptor>(out var theStartupEncryptor,
            withDpApi: true, lifetime: ServiceLifetime.Scoped);
        startupEncryptor = theStartupEncryptor;
        return innerInvocation;
    }

    /// <summary>
    /// Registers an <see cref="IPersistedEncryptor"/> to the service collection, with given type params.
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="startupEncryptor">Will return an initial <see cref="IPersistedEncryptor"/> that can be used inside startup methods.</param>
    /// <param name="purpose">The DPAPI provides a separate purpose as a string; which allows creating multiple providers that can separately encrypt/decrypt their own categories of files.</param>
    /// <param name="withDpApi">Also invoke the <see cref="DataProtectionServiceCollectionExtensions.AddDataProtectionServices"/> extension method. Set this to false if you need to invoke it earlier than here. When this true, this will also disable automatic key generation (invokes <see cref="DataProtectionBuilderExtensions.DisableAutomaticKeyGeneration"/>)
    /// <para>If this is set to false, and you do not invoke <see cref="DataProtectionBuilderExtensions.DisableAutomaticKeyGeneration"/> yourself, automatic key generation will still occur
    /// but errors may be logged when the key expires and data is unprotected past the expiration date. Data should still be successfully decrypted though and no unhandled exceptions thrown.</para>
    /// </param>
    /// <param name="lifetime"></param>
    /// <typeparam name="TEncryptorInterfaceType"></typeparam>
    /// <typeparam name="TEncryptorImplType"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IServiceCollection AddPersistedEncryptor<TEncryptorInterfaceType, TEncryptorImplType>(this IServiceCollection collection, out TEncryptorImplType startupEncryptor, 
        string purpose = "ProtectedAppSettings", bool withDpApi = true, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TEncryptorInterfaceType : class, IPersistedEncryptor
        where TEncryptorImplType: class, TEncryptorInterfaceType
    {
        if (purpose == null) throw new ArgumentNullException(nameof(purpose));
        if (withDpApi) {
            collection.AddDataProtection()
                .DisableAutomaticKeyGeneration();
        }
        else {
            var svcDesc = collection.FirstOrDefault(sd => sd.ServiceType == typeof(IDataProtectionBuilder));
            if (svcDesc == default || (svcDesc.ImplementationFactory == null && svcDesc.ImplementationInstance == null)) {
                throw new InvalidOperationException("Data protection has not been added to the service collection");
            }
            
            var idpb = (svcDesc.ImplementationInstance as IDataProtectionBuilder) ?? 
                       (svcDesc.ImplementationFactory?.Invoke(collection.BuildServiceProvider()) as IDataProtectionBuilder);

            idpb?.DisableAutomaticKeyGeneration();
        }
        
        switch (lifetime) {
            case ServiceLifetime.Scoped:
                collection.AddScoped<TEncryptorInterfaceType, TEncryptorImplType>(PersistentEncryptorFactory<TEncryptorImplType>);
                break;

            case ServiceLifetime.Singleton:
                collection.AddSingleton<TEncryptorInterfaceType, TEncryptorImplType>(PersistentEncryptorFactory<TEncryptorImplType>);
                break;

            default:
                collection.AddTransient<TEncryptorInterfaceType, TEncryptorImplType>(PersistentEncryptorFactory<TEncryptorImplType>);
                break;
        }

        // returns a single instance that can be used for startup logic
        startupEncryptor = PersistentEncryptorFactory<TEncryptorImplType>(collection.BuildServiceProvider());

        return collection;

        TEncryptorImplType PersistentEncryptorFactory<TEncryptorImplTypeInner>(IServiceProvider provider)
            where TEncryptorImplTypeInner: class, TEncryptorInterfaceType
        {
            var dataProvider = provider.GetRequiredService<IDataProtectionProvider>();
            var protector = dataProvider.CreatePersistedDataProtector(purpose);
            var instance = Activator.CreateInstance(typeof(TEncryptorImplTypeInner), args: new object[] { protector });
            return (TEncryptorImplType)instance;
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