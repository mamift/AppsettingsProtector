using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using System;

namespace AppsettingsProtector.Extensions;

public static class ConfigBuilderExtensions
{
    public static IConfigurationBuilder AddEncryptedJsonFile(this IConfigurationBuilder builder, IFileProvider provider,
        string path, bool optional, bool reloadOnChange)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (string.IsNullOrEmpty(path)) throw new ArgumentException("File path must be a non-empty string", nameof(path));

        return builder.AddEncryptedJsonFile(s => {
            s.FileProvider = provider;
            s.Path = path;
            s.Optional = optional;
            s.ReloadOnChange = reloadOnChange;
            s.ResolveFileProvider();
        });
    }

    public static IConfigurationBuilder AddEncryptedJsonFile(this IConfigurationBuilder builder,
        Action<EncryptedJsonConfigurationSource> configureSource)
    {
        return builder.Add(configureSource);
    }
}