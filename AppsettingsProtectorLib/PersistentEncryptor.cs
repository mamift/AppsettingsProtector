using System.IO;
using AppsettingsProtector.Extensions;
using Microsoft.AspNetCore.DataProtection;

namespace AppsettingsProtector;

public interface IEncryptor
{
    void ProtectFileAndSave(string srcFilePath, string? destinationFilePath = null);
    byte[] ProtectFileContents(string filePath);
    UnprotectResult UnprotectBytes(byte[] bytes);
    void UnprotectFileAndSave(string srcFilePath, string? destinationFilePath = null);
    UnprotectResult UnprotectFileContents(string filePath);
}

public class PersistentEncryptor : IEncryptor
{
    private readonly IPersistedDataProtector _persistedDataProtector;

    public PersistentEncryptor(IPersistedDataProtector provider)
    {
        _persistedDataProtector = provider;
    }

    public virtual UnprotectResult UnprotectFileContents(string filePath)
    {
        var fileBytes = File.ReadAllBytes(filePath);
        return _persistedDataProtector.DangerousUnprotect(fileBytes);
    }

    public UnprotectResult UnprotectBytes(byte[] bytes)
    {
        return _persistedDataProtector.DangerousUnprotect(bytes);
    }

    public virtual void UnprotectFileAndSave(string srcFilePath, string? destinationFilePath = null)
    {
        var unprotectResult = UnprotectFileContents(srcFilePath);
        var filePath = destinationFilePath ?? srcFilePath;
        File.WriteAllBytes(filePath, unprotectResult.UnprotectedBytes);
    }

    public virtual byte[] ProtectFileContents(string filePath)
    {
        var fileBytes = File.ReadAllBytes(filePath);
        return _persistedDataProtector.Protect(fileBytes);
    }

    public virtual void ProtectFileAndSave(string srcFilePath, string? destinationFilePath = null)
    {
        var protectedBytes = ProtectFileContents(srcFilePath);
        var filePath = destinationFilePath ?? srcFilePath;
        File.WriteAllBytes(filePath, protectedBytes);
    }
}