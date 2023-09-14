# Appsettings Protector

## About

This library can protect your JSON app settings using the [Microsoft.AspNetCore.DataProtection](https://www.nuget.org/packages/Microsoft.AspNetCore.DataProtection/) library.

## Quickstart

1. Install the nuget package.
1. Add a separate `protected.json` file to store your secrets or confidential information (you can name the file anything really, you don't have to even use the .json extension)
    - Make sure it's properly formed JSON, the library will use `System.Text.Json.JsonNode.Parse()` to do some basic validation.
1. 