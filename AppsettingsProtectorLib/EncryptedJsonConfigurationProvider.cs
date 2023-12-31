﻿using System;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using AppsettingsProtector.Extensions;
using Microsoft.Extensions.Configuration;
using OneOf;

namespace AppsettingsProtector
{
    public class EncryptedJsonConfigurationProvider : FileConfigurationProvider
    {
        private readonly IEncryptor? _encryptor;
        private readonly bool _encryptIfDecryptFails;

        public EncryptedJsonConfigurationProvider(EncryptedJsonConfigurationSource source, IEncryptor encryptor,
            bool encryptIfDecryptFails) : base(source)
        {
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
            _encryptIfDecryptFails = encryptIfDecryptFails;
        }

        public override void Load(Stream stream)
        {
            if (_encryptor == null) throw new ArgumentNullException(nameof(_encryptor), "Encryptor was never initialised!");
            string srcFilePath = (!string.IsNullOrWhiteSpace(Source.Path)
                ? Source.Path
                : throw new AppsettingsProtectorException("Unable to load protected JSON file - Path was not set in source file configuration provider."))!;

            var bytes = stream.ReadAsBytesToEnd();
            OneOf<UnprotectResult, UnprotectResult<string?>> unprotectResult;

            if (_encryptor is IPersistedBase64Encryptor base64Encryptor) {
                var base64Str = bytes.ToDefaultEncodingString();
                unprotectResult = base64Encryptor.UnprotectBase64String(base64Str);
            }
            else {
                unprotectResult = _encryptor.UnprotectBytes(bytes);
            }

            string asString;
            // failed might be because it's not encrypted
            if (!unprotectResult.Match(b => b.Success, s => s.Success)) {
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
            }
            else {
                string? matchedString = unprotectResult.Match(b => b.UnprotectedData.ToDefaultEncodingString(), s => s.UnprotectedData);
                if (matchedString == null) {
                    throw new AppsettingsProtectorException("Unable to decode string");
                }
                var _ = JsonNode.Parse(matchedString!);
                asString = matchedString;
            }

            Data = JsonConfigurationDictionaryParser.Parse(asString)!;
        }
    }

    public class EncryptedJsonConfigurationSource : FileConfigurationSource
    {
        private IEncryptor? _encryptor;

        public IEncryptor? Encryptor
        {
            get => _encryptor;
            set => SetEncryptor(value);
        }

        /// <summary>
        /// If decryption fails, it might be because the file isn't encrypted - this can occur the first time an app is deployed.
        /// <para>Setting this to true will try to encrypt the file if decryption fails.</para>
        /// <para>By default this is true.</para>
        /// </summary>
        public bool TryEncryptOnDecryptFailure { get; set; } = true;

        private void SetEncryptor(IEncryptor? value)
        {
            _encryptor = value;
        }

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
}