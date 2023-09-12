using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace AppsettingsProtector.Extensions;

public static class ProtectExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IPersistedDataProtector GetPersistedDataProtector(this IDataProtectionProvider provider, string purpose)
    {
        return (IPersistedDataProtector)provider.CreateProtector(purpose);
    }

    public static DangerousUnprotectResult DangerousUnprotect(this IPersistedDataProtector protector, byte[] @protected)
    {
        var unprotect = protector.DangerousUnprotect(@protected,true, out var requiresMigration, out var wasRevoked);

        return new DangerousUnprotectResult() {
            RequiresMigration = requiresMigration,
            WasRevoked = wasRevoked,
            UnprotectedBytes = unprotect
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] ToDefaultEncodingBytes(this string theString)
    {
        return Encoding.Default.GetBytes(theString);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? ToDefaultEncodingString(this byte[] bytes)
    {
        return Encoding.Default.GetString(bytes);
    }
}

