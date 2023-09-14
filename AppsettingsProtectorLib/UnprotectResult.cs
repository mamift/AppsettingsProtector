using System;

namespace AppsettingsProtector;

/// <summary>
/// This interface is best used when creating an implementation of the <see cref="IEncryptor"/>, interface or any of its child interfaces:
/// <see cref="IPersistedEncryptor"/> or <see cref="IPersistedBase64Encryptor"/>.
/// </summary>
/// <typeparam name="TData"></typeparam>
public interface IUnprotectResult<TData>
{
    TData UnprotectedData { get; init; }
    bool WasDangerous { get; init; }
    bool RequiresMigration { get; init; }
    bool WasRevoked { get; init; }
    bool Success { get; init; }
    Exception? Exception { get; init; }
}

/// <summary>
/// Default implementation of the <see cref="IUnprotectResult{TData}"/> interface; defaults to <see cref="byte"/> array.
/// </summary>
/// <param name="WasDangerous"></param>
/// <param name="RequiresMigration"></param>
/// <param name="WasRevoked"></param>
/// <param name="Success"></param>
/// <param name="Exception"></param>
public record UnprotectResult(
    bool WasDangerous, bool RequiresMigration, bool WasRevoked, bool Success, Exception? Exception) : IUnprotectResult<byte[]>
{
    public static readonly UnprotectResult Default =
        new(false, false, false, false, null) {
            UnprotectedData = Array.Empty<byte>()
        };

    public static UnprotectResult WithError(Exception? error)
    {
        return new(false, false, false, false, error) {
            UnprotectedData = Array.Empty<byte>()
        };
    }

    public byte[] UnprotectedData { get; init; } = null!;
}

/// <summary>
/// Generic implementation of the <see cref="IUnprotectResult{TData}"/> interface; use whatever type best fits.
/// </summary>
/// <typeparam name="TData"></typeparam>
/// <param name="WasDangerous"></param>
/// <param name="RequiresMigration"></param>
/// <param name="WasRevoked"></param>
/// <param name="Success"></param>
/// <param name="Exception"></param>
public record UnprotectResult<TData>(
    bool WasDangerous, bool RequiresMigration, bool WasRevoked, bool Success, Exception? Exception) : IUnprotectResult<TData>
{
    public static readonly UnprotectResult<TData?> Default =
        new(false, false, false, false, null) {
            UnprotectedData = default(TData)
        };

    public static UnprotectResult<TData?> WithError(Exception? error)
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