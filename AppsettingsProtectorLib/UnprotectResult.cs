using System;
using System.ComponentModel;

namespace AppsettingsProtector;

public record UnprotectResult(
    byte[] UnprotectedBytes, bool WasDangerous, bool RequiresMigration, bool WasRevoked, bool Success)
{
    public static readonly UnprotectResult Default =
        new UnprotectResult(Array.Empty<byte>(), false, false, false, false);
}