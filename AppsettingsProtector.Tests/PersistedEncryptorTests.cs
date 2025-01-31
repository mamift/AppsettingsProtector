using System.IO;
using System.Threading.Tasks;
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
        var protector = provider.CreatePersistedDataProtector("ProtectedAppSettings");
        var e = new PersistedEncryptor(protector);

        var appSettingsFile = new FileInfo("appsettings.json");

        e.UnprotectFileAndSave(appSettingsFile.FullName);

        var text = File.ReadAllText(appSettingsFile.FullName);

        Assert.NotNull(text);
        Assert.NotEmpty(text);
    }

    [Fact]
    public async Task TestEncryptorProtectFileAsync()
    {
        var provider = DataProtectionProvider.Create(BaseTester.AppName);
        var protector = provider.CreatePersistedDataProtector("ProtectedAppSettings");
        var e = new PersistedEncryptor(protector);

        var appSettingsFile = new FileInfo("appsettings.json");

        await e.ProtectFileAndSaveAsync(appSettingsFile.FullName);

        var unprotectFileContents = e.UnprotectFileContents(appSettingsFile.FullName);

        Assert.NotNull(unprotectFileContents);
        Assert.NotNull(unprotectFileContents.UnprotectedData);
        Assert.NotEmpty(unprotectFileContents.UnprotectedData);

        var unprotectedString = unprotectFileContents.UnprotectedData.ToDefaultEncodingString();
        Assert.NotNull(unprotectedString);
        Assert.NotEmpty(unprotectedString);
    }

    [Fact]
    public void TestEncryptorProtectFile()
    {
        var provider = DataProtectionProvider.Create(BaseTester.AppName);
        var protector = provider.CreatePersistedDataProtector("ProtectedAppSettings");
        var e = new PersistedEncryptor(protector);

        var appSettingsFile = new FileInfo("appsettings.json");

        e.ProtectFileAndSave(appSettingsFile.FullName);

        var unprotectFileContents = e.UnprotectFileContents(appSettingsFile.FullName);
        Assert.NotNull(unprotectFileContents);
        Assert.NotNull(unprotectFileContents.UnprotectedData);
        Assert.NotEmpty(unprotectFileContents.UnprotectedData);

        var unprotectedString = unprotectFileContents.UnprotectedData.ToDefaultEncodingString();
        Assert.NotNull(unprotectedString);
        Assert.NotEmpty(unprotectedString);
    }
}