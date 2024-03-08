using System;
using System.IO;

namespace AppsettingsProtector.Extensions;

/// <summary>
/// General extension methods.
/// </summary>
public static class GeneralExtensions
{
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