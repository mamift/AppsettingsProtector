using System.IO;
using System.Security.Cryptography;
using System.Text;
using AppsettingsProtector.Extensions;
using Microsoft.AspNetCore.DataProtection;

namespace AppsettingsProtector;

/// <summary>
/// Extenders to this library should probably use the <see cref="IPersistedEncryptor"/> interface instead of this.
/// </summary>
public interface IEncryptor
{
    /// <summary>
    /// Protects the contents of the given <paramref name="srcFilePath"/> and saves it back to either the original file path (overwriting it) or an optional
    /// <paramref name="destinationFilePath"/>.
    /// </summary>
    /// <param name="srcFilePath">The file whose contents are to be protected.</param>
    /// <param name="destinationFilePath">Optional; set this to a value to create an encrypted copy of the original.</param>
    void ProtectFileAndSave(string srcFilePath, string? destinationFilePath = null);
    /// <summary>
    /// Loads the contents of the given <paramref name="filePath"/> and returns an encrypted version of it.
    /// <para>THIS DOES NOT SAVE IT BACK TO THE FILE PATH.</para>
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    byte[] ProtectFileContents(string filePath);
    /// <summary>
    /// Protect the given <paramref name="plainText"/> as an encrypted <see cref="byte"/> array.
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    byte[] ProtectString(string plainText);
    /// <summary>
    /// Unprotect the given <paramref name="bytes"/>, and returns an instance of an <see cref="UnprotectResult{TData}"/>
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    UnprotectResult UnprotectBytes(byte[] bytes);
    /// <summary>
    /// Loads the encrypted contents of the given <paramref name="srcFilePath"/>, decrypts it and then saves it back to either the original or the
    /// <paramref name="destinationFilePath"/>.
    /// </summary>
    /// <param name="srcFilePath"></param>
    /// <param name="destinationFilePath"></param>
    void UnprotectFileAndSave(string srcFilePath, string? destinationFilePath = null);
    /// <summary>
    /// Loads the encrypted contents of the given <paramref name="filePath"/> and decrypts it.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    UnprotectResult UnprotectFileContents(string filePath);
}

/// <summary>
/// <para>Represents an encryptor that persists its key indefinitely.</para>
/// <para>Implementors should inject an instance of the <see cref="IPersistedDataProtector"/> from the Microsoft DataProtection library.</para>
/// <para>While this interface simply inherits <see cref="IEncryptor"/>, and adds nothing more, extenders should implement this interface instead of just <see cref="IEncryptor"/>
/// as the use of this interface will at least communicate that unprotect attempts will not throw an exception when the key is expired.</para>
/// </summary>
public interface IPersistedEncryptor: IEncryptor { }

/// <summary>
/// Requires an instance of an <see cref="IPersistedDataProtector"/>.
/// </summary>
public class PersistedEncryptor : IPersistedEncryptor
{
    protected readonly IPersistedDataProtector PersistedDataProtector;

    public PersistedEncryptor(IPersistedDataProtector provider)
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
        File.WriteAllBytes(filePath, unprotectResult.UnprotectedData);
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