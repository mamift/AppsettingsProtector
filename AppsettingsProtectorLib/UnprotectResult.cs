namespace AppsettingsProtector;

public record UnprotectResult(byte[] UnprotectedBytes, bool WasDangerous, bool RequiresMigration, bool WasRevoked)
{
}