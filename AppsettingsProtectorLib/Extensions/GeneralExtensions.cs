using OneOf;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace AppsettingsProtector.Extensions;

/// <summary>
/// General extension methods.
/// </summary>
public static class GeneralExtensions
{
    /// <summary>
    /// Takes a base64-encoded string and decodes it into another string.
    /// </summary>
    /// <param name="base64Str"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string DecodeBase64StringAsString(this string base64Str, Encoding? encoding = default)
    {
        var bytes = Convert.FromBase64String(base64Str);
        var decoded = bytes.ToEncodingString(encoding ?? Encoding.Default);
        return decoded;
    }

    /// <summary>
    /// Gets the possible exception that may have occured.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Exception? GetPossibleException(this OneOf<UnprotectResult, UnprotectResult<string?>> result)
    {
        return result.Match(r => r.Exception, gr => gr.Exception);
    }

    /// <summary>
    /// Resets the position of the current stream.
    /// </summary>
    /// <param name="stream"></param>
    public static void ResetPosition(this Stream stream)
    {
        if (stream.Position != default) {
            stream.Position = 0;
        }
    }

    /// <summary>
    /// Reads the current stream as a string.
    /// <para>Does not dispose of the stream.</para>
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ReadAsStringToEnd(this Stream input)
    {
        input.ResetPosition();
        var reader = new StreamReader(input);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Reads the current stream as a byte <see cref="Array"/>.
    /// <para>Does not dispose of the stream.</para>
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static byte[] ReadAsBytesToEnd(this Stream input)
    {
        input.ResetPosition();
        using MemoryStream ms = new MemoryStream();
        input.CopyTo(ms);
        return ms.ToArray();
    }
}