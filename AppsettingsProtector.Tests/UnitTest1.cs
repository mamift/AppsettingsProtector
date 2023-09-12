using System.IO;
using System.Text;
using System.Threading.Tasks;
using AppsettingsProtector.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Xunit;
using Xunit.Sdk;

namespace AppsettingsProtector.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task PersistUnprotectTest()
        {
            var provider = DataProtectionProvider.Create(nameof(AppsettingsProtector));

            IPersistedDataProtector protector = provider.GetPersistedDataProtector("AppSettings");

            if (!File.Exists("protected.txt")) {
                throw new FalseException("File does not exist!", default);
            }

            var @protected = await File.ReadAllBytesAsync("protected.txt");

            var unprotectResult = protector.DangerousUnprotect(@protected);

            Assert.NotNull(unprotectResult);
            
            var unprotectedString = Encoding.Default.GetString(unprotectResult.UnprotectedBytes);
            Assert.Equal(unprotectedString, "iamzim");
        }

        [Fact]
        public async Task PersistProtectTest()
        {
            var provider = DataProtectionProvider.Create(nameof(AppsettingsProtector));
            
            var protector = provider.GetPersistedDataProtector("AppSettings");

            byte[] plaintextBytes = Encoding.Default.GetBytes("iamzim");
            var @protected = protector.Protect(plaintextBytes);

            Assert.NotEmpty(@protected);

            await File.WriteAllBytesAsync("protected.txt", @protected);
        }
    }
}