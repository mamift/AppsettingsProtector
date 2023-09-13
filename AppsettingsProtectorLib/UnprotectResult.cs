using System;

namespace AppsettingsProtector;

public interface IUnprotectResult<TData>
{
    TData UnprotectedData { get; init; }
    bool WasDangerous { get; init; }
    bool RequiresMigration { get; init; }
    bool WasRevoked { get; init; }
    bool Success { get; init; }
    Exception? Exception { get; init; }
}

public record UnprotectResult(
    bool WasDangerous, bool RequiresMigration, bool WasRevoked, bool Success, Exception? Exception) : IUnprotectResult<byte[]>
{
    public static readonly UnprotectResult Default =
        new(false, false, false, false, null) {
            UnprotectedData = Array.Empty<byte>()
        };

    public static UnprotectResult WithError(Exception error)
    {
        return new(false, false, false, false, error) {
            UnprotectedData = Array.Empty<byte>()
        };
    }

    public byte[] UnprotectedData { get; init; } = null!;
}

public record UnprotectResult<TData>(
    bool WasDangerous, bool RequiresMigration, bool WasRevoked, bool Success, Exception? Exception) : IUnprotectResult<TData>
{
    public static readonly UnprotectResult<TData?> Default =
        new(false, false, false, false, null) {
            UnprotectedData = default(TData)
        };

    public static UnprotectResult<TData?> WithError(Exception error)
    {
        return new(false, false, false, false, error) {
            UnprotectedData = default(TData)
        };
    }
    
    public static UnprotectResult<TData> WithSuccessData(TData data)
    {
        return new(false, false, false, true, null) {
            UnprotectedData = data
        };
    }

    public TData UnprotectedData { get; init; } = default!;
}