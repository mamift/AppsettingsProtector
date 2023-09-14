# Appsettings Protector

## About

This library can protect your JSON app settings using the [Microsoft.AspNetCore.DataProtection](https://www.nuget.org/packages/Microsoft.AspNetCore.DataProtection/) library.

## Quickstart

1. Install the nuget package.
1. Add a separate `protected.json` file to store your secrets or confidential information (you can name the file anything really, you don't have to even use the .json extension)
    - Make sure it's properly formed JSON, the library will use `System.Text.Json.JsonNode.Parse()` to do some basic validation.
1. Call two extension methods on your program's host's:
    - On the host service collection: `AddPersistedEncryptor()`
        - This will register the encryptor used to encrypt the file the first time.
    - And the host configuration builder: `AddEncryptedJsonFile()`
        - This will register a new config source that will transparently decrypt the encrypted JSON file on program startup.
```C#
public static void Main(string[] args)
{
    // this example uses a Blazor web app (.NET 6 - so no Startup class)
    var builder = WebApplication.CreateBuilder(args);
    // first method        
    builder.Services.AddPersistedEncryptor<IPersistedBase64Encryptor, PersistedBase64Encryptor>(out var startupEncryptor);

    builder.Configuration.AddEncryptedJsonFile(source => {
        source.Path = "protectedSettings.json";
        source.Encryptor = startupEncryptor;
        source.TryEncryptOnDecryptFailure = true; // this is true anyway, but code is here to demonstrate the api exists
    });

    // should now be able to access encrypted JSON values!
    var secret = builder.Configuration["secret"];
    if (secret == null) {
        throw new ArgumentNullException(nameof(secret));
    }
}

```

### FAQs

1. How does it handle first-time encryption?
    - This logic is: always attempt decryption every time on startup, but if it fails, try reading it as plain text JSON and if no errors occur, assume it's un-encrypted plain text and then encrypt it.
2. What are the defaults?
    - Uses the [ASP.NET Core Data Protection library](https://www.nuget.org/packages/Microsoft.AspNetCore.DataProtection/), but disables automatic key generation, so the key is not rotated every 90 days. On Windows the key itself is protected using the Windows CN-DPAPI (so the key is accessed via currently logged in user's SID).