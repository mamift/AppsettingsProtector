using System;
using System.IO;
using System.Security.Cryptography;
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
            return UnprotectResult.Default;
        }
    }

    public UnprotectResult UnprotectBytes(byte[] bytes)
    {
        try {
            return PersistedDataProtector.DangerousUnprotect(bytes);
        } 
        catch (CryptographicException ce) {
            return UnprotectResult.Default;
        }
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
        return PersistedDataProtector.Protect(fileBytes);
    }

    public virtual void ProtectFileAndSave(string srcFilePath, string? destinationFilePath = null)
    {
        var protectedBytes = ProtectFileContents(srcFilePath);
        var filePath = destinationFilePath ?? srcFilePath;
        File.WriteAllBytes(filePath, protectedBytes);
    }
}