# Appsettings Protector

## About

This library can protect your JSON app settings using the [Microsoft.AspNetCore.DataProtection](https://www.nuget.org/packages/Microsoft.AspNetCore.DataProtection/) library.

The library itself targets .NET Standard 2, and has been tested on .NET 5, .NET 6 blazor web apps.

## Quickstart

1. Install the nuget package (link to-be-published).
1. Add a separate JSON file (e.g. `protected.json`) to store your secrets or confidential information (you can name the file anything really, you don't have to even use the .json extension) - but make sure to reference this file name in your startup code.
    - You *can* just use a single `appsettings.json` but some application settings do not need to be encrypted, like logging settings, as it complicates startup logic. For most apps I've found it easier to have a separate `project.json` file to store your secrets. Multiple files can contribute setting values to the same configuration dictionary, so long as the keys don't clash.
    - Make sure it's properly formed JSON, the library will use `System.Text.Json.JsonNode.Parse()` to do some basic validation.
    - Your application's user account needs write access to the JSON that file will be protected. You may need to add additional file-system-level access control list settings for the protected file, especially in the case for IIS-hosted apps.
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
    - The library will never write anything back to the file after the first time encryption; it only read from it, but this first-time encryption process will require you app to have file-system-level write access to the file.
2. What are the defaults?
    - Uses the [ASP.NET Data Protection library](https://www.nuget.org/packages/Microsoft.AspNetCore.DataProtection/), but:
        - Disables automatic key generation, so the key is not rotated every 90 days, but this can be overriden by you, so the library will also...
        - ...default to using the [DangerousUnprotect()](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.dataprotection.ipersisteddataprotector.dangerousunprotect?view=aspnetcore-6.0) API.
        - On Windows, the key is managed using the defaults set by the [ASP.NET Data Protection Library](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/default-settings?view=aspnetcore-6.0#key-management), which includes special accommodation for IIS-hosted apps.
3. Can I encrypt the protected JSON file during or before deployment?
    - Currently, no: as it is, the library is designed to encrypt the file for you after deployment, when the app starts up. Then it will transparently decrypt the file each time the app restarts.
    - This means post deployment and before the app starts up (like responding to a first request), there is a small time window when the JSON file itself resides as plaintext on the file system.
4. What happens if the data in the protected JSON file changes?
    - Then you must deploy an updated, plain text version of the new JSON file, then deploy, then wait for startup logic to run to re-encrypt the file.
    - Again, the same (small) time window between post-deployment and app startup will remain whereby the JSON file resides as plain text on the deployed server file system.
5. Is this a 100% secure solution?
    - No. [See below.](#vulnerable-window)
    
## Advanced security considerations + existing vulnerabilities

### Vulnerable window

There is a **vulnerable window** that exists: from post-deployment to app-startup whereby your secrets inside your JSON file are completely unencrypted. This can be between anywhere from milliseconds to minutes depending on how your app is configured.

- Because the encryption runs inside your app's startup method (usually `Main()`) while the host is being initialised, how big the window lasts depends on how much code/logic precedes the encryption process. If encryption happens first up, the window is smaller, otherwise, if any logging setup or database migrations happen before this, obviously the window is larger.
- To minimise this, include in your deployment process, a final step to kickstart the app, by sending it a throwaway HTTP request.
- **However, this is still better than nothing!**, especially for apps that reside on standalone web servers (that aren't domain managed), and there is no existing solution for protecting secret information such as connection strings. Using this library library at least ensures that for the vast majority of the lifetime that your app is running, the contents of your JSON file are concealed even to elevated/admin users.
    - And yes, technically a persistent hacker could eventually get a hold of the decryption keys, if they could exploit some 0-day or elevation vulnerability in the underlying OS, but they would need to specifically execute untrusted code on the same server (and impersonate the application's user account), and that point you've probably got bigger issues.

### File system snapshotting

Some deployment processes might take a snapshot of the file system for backup purposes, or as a precautionary measure for immediate rollback. On Windows, this is handled by the VSS (Volumn Shadow Copy) service, on Linux this is handled by the Logical Volume Manager and on MacOS systems (with HFS+ or APFS), this is handled by the local snapshot feature of Time Machine.

To mimimise capturing unencrypted data, ensure that any snapshot is taken after encrpytion of the file occurs! Again, best to include a separate deployment step that warms up the app so the encryption process happens as soon as possible after deployment.

## Desiderata (Latin for "things desired")

* Enable logging for decrypt/encrypt events using generic `ILogger<T>` interface (requires a bootstrap logger)
* Enable using your own key, via an environment variable or one of the facilities (file system, certificate or registry key [Windows only] provided by the DP API library).
* Separate console app that can encrypt the protected JSON file as a separate step? Would have to accommodate different ways different OS's and container technologies execute code under different user accounts.