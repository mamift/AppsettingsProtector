using System;
using System.IO;
using System.Text;
using AppsettingsProtector.Extensions;
using Microsoft.Extensions.Configuration;

namespace AppsettingsProtector
{
    public class EncryptedJsonConfigurationProvider : FileConfigurationProvider
    {
        private readonly IEncryptor? _encryptor;

        public EncryptedJsonConfigurationProvider(EncryptedJsonConfigurationSource source, IEncryptor encryptor) : base(source)
        {
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
        }

        public override void Load(Stream stream)
        {
            if (_encryptor == null) throw new ArgumentNullException(nameof(_encryptor), "Encryptor was never initialised!");

            var bytes = stream.ReadAsBytesToEnd();
            var unprotectResult = _encryptor.UnprotectBytes(bytes);

            string asString;
            // failed might be because it's not encrypted
            if (!unprotectResult.Success) {
                asString = stream.ReadAsStringToEnd();
            }
            else {
                asString = Encoding.Default.GetString(unprotectResult.UnprotectedBytes);
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

        private void SetEncryptor(IEncryptor? value)
        {
            _encryptor = value;
        }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            var theEncryptor = Encryptor ?? throw new InvalidOperationException("Encryptor was never initialised!");
            return new EncryptedJsonConfigurationProvider(this, theEncryptor);
        }
    }
}