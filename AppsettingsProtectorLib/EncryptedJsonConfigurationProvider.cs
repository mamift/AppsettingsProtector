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

            var bytes = stream.ReadAllBytes();
            var unprotectResult = _encryptor.UnprotectBytes(bytes);

            var asString = Encoding.Default.GetString(unprotectResult.UnprotectedBytes);
        }
    }

    public class EncryptedJsonConfigurationSource : FileConfigurationSource
    {
        private readonly IEncryptor? _encryptor;
        public EncryptedJsonConfigurationSource(IEncryptor encryptor)
        {
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
        }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            var theEncryptor = _encryptor ?? throw new InvalidOperationException("Encryptor was never initialised!");
            return new EncryptedJsonConfigurationProvider(this, theEncryptor);
        }
    }
}