using System.IO;
using System.Security.Cryptography;
using System.Text;
using AppsettingsProtector.Extensions;
using Microsoft.AspNetCore.DataProtection;

namespace AppsettingsProtector;

public interface IEncryptor
{
    /// <summary>
    /// Protects the contents of the given <paramref name="srcFilePath"/> and saves it back to either the original file path or an optional
    /// <paramref name="destinationFilePath"/>.
    /// </summary>
    /// <param name="srcFilePath"></param>
    /// <param name="destinationFilePath">Optional; set this to a value to create an encrypted copy of the original.</param>
    void ProtectFileAndSave(string srcFilePath, string? destinationFilePath = null);
    /// <summary>
    /// Loads the contents of the given <paramref name="filePath"/> and returns an encrypted version of it.
    /// <para>DOES NOT SAVE IT BACK TO THE FILE PATH.</para>
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    byte[] ProtectFileContents(string filePath);
    byte[] ProtectString(string plainText);
    UnprotectResult UnprotectBytes(byte[] bytes);
    void UnprotectFileAndSave(string srcFilePath, string? destinationFilePath = null);
    UnprotectResult UnprotectFileContents(string filePath);
}

/// <summary>
/// Implementors should inject an instance of the <see cref="IPersistedDataProtector"/> from the Microsoft DataProtection library.
/// </summary>
public interface IPersistentEncryptor: IEncryptor { }

/// <summary>
/// Requires an instance of an <see cref="IPersistedDataProtector"/>.
/// </summary>
public class PersistentEncryptor : IPersistentEncryptor
{
    protected readonly IPersistedDataProtector PersistedDataProtector;

    public PersistentEncryptor(IPersistedDataProtector provider)
    {
        PersistedDataProtector = provider;
    }

    public virtual UnprotectResult UnprotectFileContents(string filePath)
    {
        var fileBytes = File.ReadAllBytes(filePath);
        try {
            return PersistedDataProtector.DangerousUnprotect(fileBytes);
        }
        catch (CryptographicException ce) {
            return UnprotectResult.WithError(ce);
        }
    }

    /// <summary>
    /// This will encode the given <paramref name="plainText"/> string as a byte array using <see cref="Encoding.GetBytes(char*,int,byte*,int)"/> method,
    /// choosing the <see cref="Encoding.Default"/> instance, which is OS-determined.
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    public virtual byte[] ProtectString(string plainText)
    {
        var stringBytes = Encoding.Default.GetBytes(plainText);
        var @protected = PersistedDataProtector.Protect(stringBytes);
        return @protected;
    }

    public virtual UnprotectResult UnprotectBytes(byte[] bytes)
    {
        try {
            return PersistedDataProtector.DangerousUnprotect(bytes);
        } 
        catch (CryptographicException ce) {
            return UnprotectResult.WithError(ce);
        }
    }

    public virtual void UnprotectFileAndSave(string srcFilePath, string? destinationFilePath = null)
    {
        var unprotectResult = UnprotectFileContents(srcFilePath);
        if (unprotectResult is { Success: false, Exception: not null }) throw unprotectResult.Exception;
        var filePath = destinationFilePath ?? srcFilePath;
        File.WriteAllBytes(filePath, unprotectResult.UnprotectedBytes);
    }

    public virtual byte[] ProtectFileContents(string filePath)
    {
        var fileBytes = File.ReadAllBytes(filePath);
        return PersistedDataProtector.Protect(fileBytes);
    }

    public virtual void ProtectFileAndSave(string srcFilePath, string? destinationFilePath = null)
    {
        var protectedBytes = ProtectFileContents(srcFilePath);
        var filePath = destinationFilePath ?? srcFilePath;
        File.WriteAllBytes(filePath, protectedBytes);
    }
}