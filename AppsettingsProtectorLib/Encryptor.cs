using Microsoft.AspNetCore.DataProtection;

namespace AppsettingsProtector
{
    public class Encryptor
    {
        private readonly IDataProtectionProvider _provider;

        public Encryptor(IDataProtectionProvider provider)
        {
            _provider = provider;
        }
    }
}