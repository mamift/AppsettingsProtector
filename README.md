# Appsettings Protector

## About

This library can protect your JSON app settings using the [Microsoft.AspNetCore.DataProtection](https://www.nuget.org/packages/Microsoft.AspNetCore.DataProtection/) library.

## Quickstart

1. Install the nuget package.
1. Add a separate `protected.json` file to store your secrets or confidential information (you can name the file anything really, you don't have to even use the .json extension)
    - Make sure it's properly formed JSON, the library will use `System.Text.Json.JsonNode.Parse()` to do some basic validation.
1. Call two extension methods on your program's host's:
    - Service collection: `AddPersistedEncryptor()`
        - This will register the encryptor,
    - And configuration builder: `AddEncryptedJsonFile()`
```C#
public static void Main(string[] args)
{
    // this example uses a Blazor web app (.NET 6)
    var builder = WebApplication.CreateBuilder(args);
    // first method        
    builder.Services.AddPersistedEncryptor<IPersistedBase64Encryptor, PersistedBase64Encryptor>(out var startupEncryptor);

    builder.Configuration.AddEncryptedJsonFile(source => {
        source.Path = "protectedSettings.json";
        source.Encryptor = startupEncryptor;
        source.TryEncryptOnDecryptFailure = true; // this is true anyway, but code is here to demonstrate the api exists
    });

    var secret = builder.Configuration["secret"];
    if (secret == null) {
        throw new ArgumentNullException(nameof(secret));
    }
}

```