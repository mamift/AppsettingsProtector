using System;
using System.IO;
using System.Text.Json.Nodes;
using AppsettingsProtector.Extensions;
using Microsoft.Extensions.Configuration;
using OneOf;

namespace AppsettingsProtector;

/// <summary>
/// A configuration provider that can encrypt/decrypt JSON files.
/// </summary>
public class EncryptedJsonConfigurationProvider : FileConfigurationProvider
{
    private readonly IEncryptor? _encryptor;
    private readonly bool _encryptIfDecryptFails;

    public bool HasBeenEncryptedFromPlainText { get; set; }

    /// <summary>
    /// Instantiates a new instance with a <paramref name="source"/> (<see cref="EncryptedJsonConfigurationSource"/>) and an <paramref name="encryptor"/>.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="encryptor"></param>
    /// <param name="encryptIfDecryptFails">Set to <c>true</c> to handle a failed decryption exception and just encrypt the file.
    /// This is used to encrypt on the first run of an app after a fresh deployment.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public EncryptedJsonConfigurationProvider(EncryptedJsonConfigurationSource source, IEncryptor encryptor,
        bool encryptIfDecryptFails) : base(source)
    {
        _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
        _encryptIfDecryptFails = encryptIfDecryptFails;
    }

    /// <summary>
    /// Load config file from a <see cref="Stream"/> given by the config builder.
    /// </summary>
    /// <param name="stream"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="AppsettingsProtectorException"></exception>
    public override void Load(Stream stream)
    {
        if (_encryptor == null) throw new ArgumentNullException(nameof(_encryptor), "Encryptor was never initialised!");
        string srcFilePath = (!string.IsNullOrWhiteSpace(Source.Path)
            ? Source.Path
            : throw new AppsettingsProtectorException("Unable to load protected JSON file - Path was not set in source file configuration provider."))!;

        var bytes = stream.ReadAsBytesToEnd();
        OneOf<UnprotectResult, UnprotectResult<string?>> unprotectResult;

        // if the encrypt is a base64 one, then the bytes are actually a base64 string.
        if (_encryptor is IPersistedBase64Encryptor base64Encryptor) {
            var base64Str = bytes.ToDefaultEncodingString();
            unprotectResult = base64Encryptor.UnprotectBase64String(base64Str);
        }
        else {
            unprotectResult = _encryptor.UnprotectBytes(bytes);
        }

        string asString;
        // failed might be because it's not encrypted
        var successFlag = unprotectResult.Match(b => b.Success, s => s.Success);
        Exception? possibleError = unprotectResult.Match(r => r.Exception, r => r.Exception);
        if (!successFlag && (possibleError is FormatException || (possibleError is AppsettingsProtectorException &&
                                                                  possibleError.Message.StartsWith("Not actually encrypted")))) {
            asString = stream.ReadAsStringToEnd();
            // check if the string is valid json
            var _ = JsonNode.Parse(asString);

            if (_encryptIfDecryptFails) {
                if (_encryptor is IPersistedBase64Encryptor base64Encryptor2) {
                    base64Encryptor2.ProtectFileAndSave(srcFilePath);
                }
                else {
                    _encryptor.ProtectFileAndSave(srcFilePath);
                }
            }
            else {
                var possibleException = unprotectResult.Match(b => b.Exception, s => s.Exception);
                throw new AppsettingsProtectorException("Decryption failed!", possibleException);
            }
        }
        else {
            string? matchedString = unprotectResult.Match(b => b.UnprotectedData.ToDefaultEncodingString(), s => s.UnprotectedData);
            if (matchedString == null) {
                throw new AppsettingsProtectorException("Unable to decode string");
            }
            var _ = JsonNode.Parse(matchedString);
            asString = matchedString;
        }

        Data = JsonConfigurationDictionaryParser.Parse(asString);
    }
}

/// <summary>
/// Represents an encrypted JSON configuration source. Requires an <see cref="Encryptor"/>.
/// </summary>
public class EncryptedJsonConfigurationSource : FileConfigurationSource
{
    private IEncryptor? _encryptor;

    /// <summary>
    /// The encryptor used to encrypt/decrypt the configuration file.
    /// </summary>
    public IEncryptor? Encryptor
    {
        get => _encryptor;
        set => SetEncryptor(value);
    }

    /// <summary>
    /// If decryption fails, it might be because the file isn't encrypted - this can occur the first time an app is deployed.
    /// <para>Setting this to true will try to encrypt the file if decryption fails.</para>
    /// <para>By default this is true.</para>
    /// <para>Set this to <c>false</c> to diagnose any decryption errors. This is due to the DP API throwing the same exception for: 1. a decryption attempt
    /// on a plain text unencrypted file, and 2. a decryption attempt on an actually encrypted file but the key has been rotated or lost.</para>
    /// <para>If this is <c>true</c> and the file is actually encrypted, the library will double-encrypt the file.</para>
    /// </summary>
    public bool TryEncryptOnDecryptFailure { get; set; } = true;

    private void SetEncryptor(IEncryptor? value)
    {
        _encryptor = value;
    }

    /// <summary>
    /// Builds and return the <see cref="IConfigurationBuilder"/>, specifically an instance of the <see cref="EncryptedJsonConfigurationProvider"/>.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        base.EnsureDefaults(builder);
        if (string.IsNullOrWhiteSpace(Path)) {
            throw new ArgumentNullException(nameof(Path), "No path was provided! Please explicitly provide a path to the JSON file to protect.");
        }
        var theEncryptor = Encryptor ?? throw new InvalidOperationException("Encryptor was never initialised!");
        return new EncryptedJsonConfigurationProvider(this, theEncryptor, TryEncryptOnDecryptFailure);
    }
}