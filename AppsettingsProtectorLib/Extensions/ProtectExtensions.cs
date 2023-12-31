﻿using System;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace AppsettingsProtector.Extensions;

public static class ProtectExtensions
{
    /// <summary>
    /// Will instantiate a new instance of a data protector using the <see cref="IPersistedDataProtector"/> interface.
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="purpose"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IPersistedDataProtector CreatePersistedDataProtector(this IDataProtectionProvider provider, string purpose)
    {
        return (IPersistedDataProtector)provider.CreateProtector(purpose);
    }

    public static UnprotectResult DangerousUnprotect(this IPersistedDataProtector protector, byte[] @protected)
    {
        var unprotect = protector.DangerousUnprotect(@protected,true, out var requiresMigration, out var wasRevoked);

        return new UnprotectResult(WasDangerous: true, RequiresMigration: requiresMigration, WasRevoked: wasRevoked,
            Success: unprotect.Length > 0, Exception: null) {
            UnprotectedData = unprotect
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] ToDefaultEncodingBytes(this string theString)
    {
        return Encoding.Default.GetBytes(theString);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] FromBase64String(this string theString)
    {
        return Convert.FromBase64String(theString);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToBase64String(this byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToDefaultEncodingString(this byte[] bytes)
    {
        return Encoding.Default.GetString(bytes);
    }
}

