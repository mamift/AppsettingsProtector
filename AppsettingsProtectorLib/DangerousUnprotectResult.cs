namespace AppsettingsProtector;

public record DangerousUnprotectResult
{
    public byte[] UnprotectedBytes { get; init; } = null!;
    public bool RequiresMigration { get; init; }
    public bool WasRevoked { get; init; }
}