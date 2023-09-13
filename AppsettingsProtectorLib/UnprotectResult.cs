using System;

namespace AppsettingsProtector;

public record UnprotectResult(
    byte[] UnprotectedBytes, bool WasDangerous, bool RequiresMigration, bool WasRevoked, bool Success, Exception? Exception)
{
    public static readonly UnprotectResult Default = 
        new(Array.Empty<byte>(), false, false, false, false, null);

    public static UnprotectResult WithError(Exception error)
    {
        return new(Array.Empty<byte>(), false, false, false, false, error);
    }
}