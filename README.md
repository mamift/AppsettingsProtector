# Appsettings Protector

## About

This library can protect your JSON app settings using the [Microsoft.AspNetCore.DataProtection](https://www.nuget.org/packages/Microsoft.AspNetCore.DataProtection/) library.

The library itself targets .NET Standard 2, and has been tested on .NET 5, .NET 6 blazor web apps.

## Quickstart

1. Install the nuget package (link to-be-published).
1. Add a separate JSON file (e.g. `protected.json`) to store your secrets or confidential information (you can name the file anything really, you don't have to even use the .json extension) - but make sure to reference this file name in your startup code.
    - You *can* just use a single `appsettings.json` but some application settings do not need to be encrypted, like logging settings, as it complicates startup logic. For most apps I've found it easier to have a separate `project.json` file to store your secrets. Multiple files can contribute setting values to the same configuration dictionary, so there shouldn't be an issue.
    - Make sure it's properly formed JSON, the library will use `System.Text.Json.JsonNode.Parse()` to do some basic validation.
1. Call **two** extension methods on your program's startup code in this order:
    - First on the host service collection: `AddPersistedEncryptor()`
        - This will register the encryptor used to encrypt the file the first time, which you will then pass to the next method.
    - Second, on the host configuration builder: `AddEncryptedJsonFile()`
        - This will register a new config source that will transparently decrypt the encrypted JSON file on program startup.

### Example implementation code:
```C#
public static void Main(string[] args)
{
    // this example uses a Blazor web app (.NET 6 - so no Startup class)
    var builder = WebApplication.CreateBuilder(args);
    // first method        
    builder.Services.AddPersistedEncryptorWithDefaults(out var startupEncryptor);
    // second method
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
    - This logic is: always attempt decryption of the specified JSON file every time on startup, but if it fails, try parsing it as plain text JSON, and if no errors occur during this JSON-parsing phase, assume it's un-encrypted and then encrypt it. 
    - This should properly handle first time encryption, and the same logic repeats every subsequent time startup occurs.
    - The library will never write anything back to the file after the first time encryption; it only read from it.
2. What are the defaults?
    - Uses the [ASP.NET Core Data Protection library](https://www.nuget.org/packages/Microsoft.AspNetCore.DataProtection/), but:
        - Disables automatic key generation, so the key is not rotated every 90 days, but this can be overriden by you, so the library will also...
        - ...default to using the [DangerousUnprotect()](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.dataprotection.ipersisteddataprotector.dangerousunprotect?view=aspnetcore-6.0) API.
        - On Windows the key used by itself is protected using the Windows DPAPI (so the key is accessed via currently logged in user's SID).
3. Can I encrypt the protected JSON file during or before deployment?
    - Currently, no: as it is, the library is designed to encrypt the file for you after deployment, when the app starts up. Then it will transparently decrypt the file each time the app restarts.
    - This means post deployment and before the app starts up (like responding to a first request), there is a small time window when the JSON file itself resides as plaintext on the file system.
4. What happens if the data in the protected JSON file changes?
    - Then you must deploy an updated, plain text version of the new JSON file, then deploy, then wait for startup logic to run to re-encrypt the file.
    - Again, the same (small) time window between post-deployment and app startup will remain whereby the JSON file resides as plain text on the deployed server file system.

## Roadmap for new features

* Enable logging for decrypt/encrypt events using generic `ILogger<T>` interface (requires a bootstrap logger)
* Enable using your own key, via an environment variable or one of the facilities (file system, certificate or registry key [Windows only] provided by the DP API library).