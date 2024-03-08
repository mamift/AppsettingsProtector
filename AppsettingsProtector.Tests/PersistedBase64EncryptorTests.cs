using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using AppsettingsProtector.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Xunit;

namespace AppsettingsProtector.Tests;

public class PersistedBase64EncryptorTests
{
    private static PersistedBase64Encryptor GetPersistedBase64Encryptor()
    {
        var provider = DataProtectionProvider.Create(BaseTester.AppName);
        var protector = provider.CreatePersistedDataProtector("ProtectedAppSettings");
        var e = new PersistedBase64Encryptor(protector);
        return e;
    }

    [Fact]
    public async Task TestProtectFileContents()
    {
        var encryptor = GetPersistedBase64Encryptor();
        var appSettingsFile = new FileInfo("appsettings.json");

        var protectedBytes = encryptor.ProtectFileContents(appSettingsFile.FullName);

        Assert.NotNull(protectedBytes);
        Assert.NotEmpty(protectedBytes);

        var plainBytes = encryptor.UnprotectBytes(protectedBytes);

        Assert.NotNull(plainBytes);
        Assert.NotNull(plainBytes.UnprotectedData);
        Assert.NotEmpty(plainBytes.UnprotectedData);

        var unprotectedString = plainBytes.UnprotectedData.ToDefaultEncodingString();
        Assert.NotNull(unprotectedString);
        Assert.NotEmpty(unprotectedString);

        var jsonDocument = JsonDocument.Parse(unprotectedString);

        Assert.NotNull(jsonDocument);
        Assert.NotEmpty(jsonDocument.RootElement.EnumerateObject());
    }

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