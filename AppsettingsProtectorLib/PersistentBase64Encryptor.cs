using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AppsettingsProtector.Extensions;
using Microsoft.AspNetCore.DataProtection;

namespace AppsettingsProtector;

public interface IPersistedBase64Encryptor : IPersistedEncryptor
{
    /// <summary>
    /// Encrypts the given <paramref name="plainText"/> and converts the encrypted bytes as a base64 string.
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    string ProtectStringAsBase64(string plainText);

    /// <summary>
    /// Unprotects base64-encoded text and returns the unencrypted bytes as a plain string.
    /// </summary>
    /// <param name="base64Text"></param>
    /// <returns></returns>
    UnprotectResult<string?> UnprotectBase64String(string base64Text);
}

public class PersistedBase64Encryptor: PersistedEncryptor, IPersistedBase64Encryptor
{
    private Base64FormattingOptions _base64FormattingOptions;

    public PersistedBase64Encryptor(IPersistedDataProtector provider) : base(provider)
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

    /// <summary>
    /// Unprotect the given <paramref name="base64Text"/>. This will catch any <see cref="FormatException"/>s and return an empty byte array, which indicates that the given text is actually not encoded or encrypted.
    /// </summary>
    /// <param name="base64Text"></param>
    /// <returns></returns>
    public UnprotectResult<string?> UnprotectBase64String(string base64Text)
    {
        byte[] bytesFromBase64String;
        try {
            bytesFromBase64String = Convert.FromBase64String(base64Text);
        }
        catch (FormatException fe) {
            bytesFromBase64String = Array.Empty<byte>();
        }

        var unprotectResult = base.UnprotectBytes(bytesFromBase64String);
        return ResolveDecodedString(unprotectResult);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private UnprotectResult<string?> ResolveDecodedString(UnprotectResult ur)
    {
        if (!ur.Success) {
            return UnprotectResult<string?>.WithError(ur.Exception);
        }
        var decodedString = ur.UnprotectedData.ToDefaultEncodingString();

        if (string.IsNullOrWhiteSpace(decodedString))
            return UnprotectResult<string?>.WithError(new Exception("Unspecified error - decoded string was empty"));

        return UnprotectResult<string?>.WithSuccessData(decodedString);
    }

    public override void ProtectFileAndSave(string srcFilePath, string? destinationFilePath = null)
    {
        using var fileStream = new FileStream(srcFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        var plainBytes = fileStream.ReadAsBytesToEnd();
        var protectedBytes = ProtectBytes(plainBytes);
        var base64 = Convert.ToBase64String(protectedBytes);
        // File.WriteAllText(filePath, base64);
        fileStream.ResetPosition();
        fileStream.SetLength(0);
        var base64Bytes = base64.ToDefaultEncodingBytes();

        if (destinationFilePath == null) {
            fileStream.Write(base64Bytes, 0, base64Bytes.Length);
            return;
        }

        File.WriteAllBytes(destinationFilePath, base64Bytes);
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