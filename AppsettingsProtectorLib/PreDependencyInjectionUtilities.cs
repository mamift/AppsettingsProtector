using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using AppsettingsProtector.Extensions;

namespace AppsettingsProtector;

/// <summary>
/// Contains setup methods and static instances for using/setting up encrypted JSON/appSettings files before DI configuration.
/// <para>Use this class if <see cref="ConfigBuilderExtensions.AddEncryptedJsonFile{TEncryptor}"/> are insufficient.</para>
/// </summary>
public static class PreDependencyInjectionUtilities
{
    private static IDataProtectionProvider? _sharedDataProtectionProvider;

    /// <summary>
    /// Sets up the <see cref="SharedDataProtectionProvider"/>, <see cref="SharedPersistedDataProtector"/> and <see cref="SharedPersistedEncryptor"/>.
    /// </summary>
    /// <param name="appName"></param>
    /// <param name="purpose"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SetupSharedDataProtectionProvider(string appName, string purpose = "ProtectedAppSettings")
    {
        if (string.IsNullOrWhiteSpace(appName)) throw new ArgumentNullException(nameof(appName));

        SharedDataProtectionProvider = DataProtectionProvider.Create(appName);
        SharedPersistedDataProtector ??= SharedDataProtectionProvider.CreatePersistedDataProtector(purpose);
        SharedPersistedEncryptor ??= SharedPersistedDataProtector.CreatePersistedBase64Encryptor();
    }

    /// <summary>
    /// The shared <see cref="IPersistedDataProtector"/> to use.
    /// </summary>
    public static IPersistedDataProtector? SharedPersistedDataProtector { get; private set; }

    /// <summary>
    /// This is the static instance of the <see cref="IDataProtectionProvider"/> that is used to encrypt app settings (or any other plain text file really).
    /// To set this up, call the <see cref="SetupSharedDataProtectionProvider"/> method, then you can create a <see cref="SharedPersistedDataProtector"/> using this
    /// instance, with <see cref="ProtectExtensions.CreatePersistedDataProtector"/> extension method.
    /// </summary>
    /// <exception cref="AppsettingsProtectorException"></exception>
    public static IDataProtectionProvider SharedDataProtectionProvider
    {
        get => _sharedDataProtectionProvider ??
               throw new AppsettingsProtectorException($"Need to call the {nameof(SetupSharedDataProtectionProvider)} before accessing this property!");
        private set => _sharedDataProtectionProvider = value;
    }

    /// <summary>
    /// The persisted encryptor to use. Using the defaults, it will setup a <see cref="IPersistedBase64Encryptor"/>.
    /// </summary>
    public static IPersistedEncryptor? SharedPersistedEncryptor { get; private set; }

    /// <summary>
    /// Sets up an encrypted JSON file for use as appSettings before configuring the DI container by statically creating the required object instances.
    /// <para>Still requires an <see cref="IConfigurationBuilder"/>.</para>
    /// </summary>
    /// <param name="configurationBuilder"></param>
    /// <param name="protectorName"></param>
    /// <param name="protectedJsonFile"></param>
    /// <param name="skipCondition">Skip encryption entirely if this evaluates to true. If this is true, then the given <paramref name="protectedJsonFile"/> file will be treated as a plain text file.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void SetupEncryptedJsonFile(IConfigurationBuilder configurationBuilder, string protectorName, string protectedJsonFile,
        Func<bool> skipCondition)
    {
        if (!File.Exists(protectedJsonFile)) throw new InvalidOperationException($"File: '{protectedJsonFile}' does not exist!");
        SharedPersistedDataProtector ??= SharedDataProtectionProvider.CreatePersistedDataProtector(protectorName);
        SharedPersistedEncryptor ??= SharedPersistedDataProtector.CreatePersistedBase64Encryptor();

        if (skipCondition()) {
            // do not encrypt if skipCondition was true
            configurationBuilder.AddJsonFile(source => {
                source.ReloadOnChange = true;
                source.Path = protectedJsonFile;
            });
            return;
        }

        configurationBuilder.AddEncryptedJsonFile(source => {
            source.ReloadOnChange = true;
            source.Encryptor = SharedPersistedEncryptor ??
                               throw new InvalidOperationException(
                                   $"{nameof(SharedPersistedEncryptor)} was not initialised!");
            source.Path = protectedJsonFile;
        });
    }
}