using System.IO;
using AppsettingsProtector.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Xunit;

namespace AppsettingsProtector.Tests;

public class PersistedEncryptorTests
{
    [Fact]
    public void TestEncryptorUnprotectFile()
    {
        var provider = DataProtectionProvider.Create(BaseTester.AppName);
        var protector = provider.CreatePersistedDataProtector("AppSettingsEncryption");
        var e = new PersistedEncryptor(protector);

        var appSettingsFile = new FileInfo("appsettings.json");
        
        e.UnprotectFileAndSave(appSettingsFile.FullName);
    }

    [Fact]
    public void TestEncryptorProtectFile()
    {
        var provider = DataProtectionProvider.Create(BaseTester.AppName);
        var protector = provider.CreatePersistedDataProtector("AppSettingsEncryption");
        var e = new PersistedEncryptor(protector);

        var appSettingsFile = new FileInfo("appsettings.json");
        
        e.ProtectFileAndSave(appSettingsFile.FullName);
    }
}