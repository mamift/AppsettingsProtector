using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsettingsProtector.Extensions;
using Microsoft.AspNetCore.DataProtection;
using NaturalSort.Extension;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace AppsettingsProtector.Tests
{
    public class BaseTester
    {
        public static readonly string AppName = nameof(AppsettingsProtector);
    }

    public class TestCaseSorter : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            return testCases.OrderByDescending(t => t.DisplayName, StringComparer.OrdinalIgnoreCase.WithNaturalSort());
        }
    }

    [TestCaseOrderer(ordererTypeName: nameof(TestCaseSorter), ordererAssemblyName: nameof(AppsettingsProtector) + "." + nameof(AppsettingsProtector.Tests))]
    public class UnitTest1 : BaseTester
    {
        [Fact]
        public async Task PersistUnprotectTest()
        {
            var provider = DataProtectionProvider.Create(AppName);

            IPersistedDataProtector protector = provider.CreatePersistedDataProtector("AppSettings");

            if (!File.Exists("protected.txt")) {
                throw new FalseException("File does not exist!", default);
            }

            var @protected = await File.ReadAllBytesAsync("protected.txt");

            var unprotectResult = protector.DangerousUnprotect(@protected);

            Assert.NotNull(unprotectResult);
            
            var unprotectedString = Encoding.Default.GetString(unprotectResult.UnprotectedData);
            Assert.Equal(unprotectedString, "iamzim");
        }

        [Fact]
        public async Task PersistProtectTest()
        {
            var provider = DataProtectionProvider.Create(AppName);
            
            var protector = provider.CreatePersistedDataProtector("AppSettings");

            byte[] plaintextBytes = Encoding.Default.GetBytes("iamzim");
            var @protected = protector.Protect(plaintextBytes);

            Assert.NotEmpty(@protected);

            await File.WriteAllBytesAsync("protected.txt", @protected);
        }
    }
}