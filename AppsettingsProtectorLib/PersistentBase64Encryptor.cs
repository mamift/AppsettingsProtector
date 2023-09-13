using System;
using System.IO;
using System.Text;
using AppsettingsProtector.Extensions;
using Microsoft.AspNetCore.DataProtection;

namespace AppsettingsProtector;

public interface IPersistentBase64Encryptor : IPersistentEncryptor
{
    /// <summary>
    /// Protects plain text and converts the encrypted bytes as a base64 string.
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    string ProtectStringAsBase64(string plainText);

    /// <summary>
    /// Unprotects base64-encoded text and returns the unencrypted bytes as a plain string.
    /// </summary>
    /// <param name="base64Text"></param>
    /// <returns></returns>
    string UnprotectBase64String(string base64Text);
}

public class PersistentBase64Encryptor: PersistentEncryptor, IPersistentBase64Encryptor
{
    private Base64FormattingOptions _base64FormattingOptions;

    public PersistentBase64Encryptor(IPersistedDataProtector provider) : base(provider)
    {
        _base64FormattingOptions = Base64FormattingOptions.InsertLineBreaks;
    }

    /// <summary>
    /// Protects the given <paramref name="plainText"/> and returns the encrypted bytes as a base64 encoded string.
    /// </summary>
    /// <param name="plainText">Base64 encoded string</param>
    /// <returns></returns>
    public string ProtectStringAsBase64(string plainText)
    {
        var baseInstance = base.ProtectString(plainText);
        _base64FormattingOptions = Base64FormattingOptions.InsertLineBreaks;
        return Convert.ToBase64String(baseInstance, _base64FormattingOptions);
    }

    public string UnprotectBase64String(string base64Text)
    {
        var bytesFromBase64String = Convert.FromBase64String(base64Text);
        var unprotected = base.UnprotectBytes(bytesFromBase64String);

        return unprotected.UnprotectedBytes.ToDefaultEncodingString();
    }

    public override void ProtectFileAndSave(string srcFilePath, string? destinationFilePath = null)
    {
        var protectedBytes = ProtectFileContents(srcFilePath);
        var base64 = Convert.ToBase64String(protectedBytes);
        var filePath = destinationFilePath ?? srcFilePath;
        File.WriteAllText(filePath, base64);
    }

    public override void UnprotectFileAndSave(string srcFilePath, string? destinationFilePath = null)
    {
        var protectedBase64Text = File.ReadAllText(srcFilePath);
        var protectedBytes = Convert.FromBase64String(protectedBase64Text);
        var unprotectResult = UnprotectBytes(protectedBytes);
        if (unprotectResult is { Success: false, Exception: not null }) throw unprotectResult.Exception;
        var filePath = destinationFilePath ?? srcFilePath;
        var defaultEncodingString = unprotectResult.UnprotectedBytes.ToDefaultEncodingString();
        File.WriteAllText(filePath, defaultEncodingString);
    }
}