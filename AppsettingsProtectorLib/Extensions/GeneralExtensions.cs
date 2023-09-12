using System.IO;

namespace AppsettingsProtector.Extensions;

public static class GeneralExtensions
{
    public static string ReadAsStringToEnd(this Stream input)
    {
        if (input.Position != default) {
            input.Position = 0;
        }
        var reader = new StreamReader(input);
        return reader.ReadToEnd();
    }

    public static byte[] ReadAsBytesToEnd(this Stream input)
    {
        if (input.Position != default) {
            input.Position = 0;
        }
        using MemoryStream ms = new MemoryStream();
        input.CopyTo(ms);
        return ms.ToArray();
    }
}