using System;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.DataProtection;

namespace AppsettingsProtector.Extensions;

public static class ProtectExtensions
{
    public static IPersistedDataProtector CreatePersistedDataProtector(string appName)
    {
        IDataProtectionProvider provider = DataProtectionProvider.Create(appName);

        if (provider is not IPersistedDataProtector persistedDataProtector) {
            throw new InvalidOperationException("Unable to create " + nameof(IPersistedDataProtector));
        }

        return persistedDataProtector;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IPersistedDataProtector GetPersistedDataProtector(this IDataProtectionProvider provider, string purpose)
    {
        return (IPersistedDataProtector)provider.CreateProtector(purpose);
    }
}