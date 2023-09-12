using System.IO;
using Microsoft.Extensions.Configuration;

namespace AppsettingsProtector
{
    public class EncryptedJsonConfigurationProvider : FileConfigurationProvider
    {
        public EncryptedJsonConfigurationProvider(FileConfigurationSource source) : base(source) { }

        public override void Load(Stream stream)
        {
            
        }
    }

    public class EncryptedJsonConfigurationSource : FileConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new EncryptedJsonConfigurationProvider(this);
        }
    }
}