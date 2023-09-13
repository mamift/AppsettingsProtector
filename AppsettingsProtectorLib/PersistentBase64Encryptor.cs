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
    UnprotectResult<string> UnprotectBase64String(string base64Text);
    UnprotectResult<string?> UnprotectBytesAsBase64String(byte[] bytes);
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

    public UnprotectResult<string> UnprotectBase64String(string base64Text)
    {
        var bytesFromBase64String = Convert.FromBase64String(base64Text);
        var unprotectResult = base.UnprotectBytes(bytesFromBase64String);
        return UnprotectResult<string>.WithSuccessData(unprotectResult.UnprotectedData.ToDefaultEncodingString());
    }

    public UnprotectResult<string?> UnprotectBytesAsBase64String(byte[] bytes)
    {
        var unprotectResult = base.UnprotectBytes(bytes);
        if (!unprotectResult.Success) {
            return UnprotectResult<string?>.WithError(unprotectResult.Exception);
        }
        var base64String = Convert.ToBase64String(unprotectResult.UnprotectedData);

        if (base64String.Length <= 0)
            return UnprotectResult<string?>.WithError(new Exception("Unspecified error - decoded base64 string was empty"));

        return UnprotectResult<string?>.WithSuccessData(base64String);
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
        var unprotectResult = UnprotectBase64String(protectedBase64Text);
        if (unprotectResult is { Success: false, Exception: not null }) throw unprotectResult.Exception;
        var filePath = destinationFilePath ?? srcFilePath;
        File.WriteAllText(filePath, unprotectResult.UnprotectedData);
    }
}