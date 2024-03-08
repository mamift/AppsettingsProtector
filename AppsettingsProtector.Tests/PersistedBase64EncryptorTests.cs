using System.IO;
using System.Threading.Tasks;
using AppsettingsProtector.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Xunit;

namespace AppsettingsProtector.Tests;

public class PersistedBase64EncryptorTests
{
    [Fact]
    public void TestEncryptorUnprotectFile()
    {
        var encryptor = GetPersistedBase64Encryptor();

        var appSettingsFile = new FileInfo("appsettings.json");

        encryptor.UnprotectFileAndSave(appSettingsFile.FullName);

        var text = File.ReadAllText(appSettingsFile.FullName);

        Assert.NotNull(text);
        Assert.NotEmpty(text);
    }

    private static PersistedBase64Encryptor GetPersistedBase64Encryptor()
    {
        var provider = DataProtectionProvider.Create(BaseTester.AppName);
        var protector = provider.CreatePersistedDataProtector("ProtectedAppSettings");
        var e = new PersistedBase64Encryptor(protector);
        return e;
    }

    [Fact]
    public async Task TestEncryptorProtectFileAsync()
    {
        var encryptor = GetPersistedBase64Encryptor();

        var appSettingsFile = new FileInfo("appsettings.json");

        await encryptor.ProtectFileAndSaveAsync(appSettingsFile.FullName);

        var unprotectFileContents = encryptor.UnprotectFileContents(appSettingsFile.FullName);

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
        var encryptor = GetPersistedBase64Encryptor();

        var appSettingsFile = new FileInfo("appsettings.json");

        encryptor.ProtectFileAndSave(appSettingsFile.FullName);

        var unprotectFileContents = encryptor.UnprotectFileContents(appSettingsFile.FullName);
        Assert.NotNull(unprotectFileContents);
        Assert.NotNull(unprotectFileContents.UnprotectedData);
        Assert.NotEmpty(unprotectFileContents.UnprotectedData);

        var unprotectedString = unprotectFileContents.UnprotectedData.ToDefaultEncodingString();
        Assert.NotNull(unprotectedString);
        Assert.NotEmpty(unprotectedString);
    }
}