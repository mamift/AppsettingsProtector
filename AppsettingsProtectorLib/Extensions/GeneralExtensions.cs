using System.IO;

namespace AppsettingsProtector.Extensions;

public static class GeneralExtensions
{
    public static byte[] ReadAllBytes(this Stream input)
    {
        if (input.Position != default) {
            input.Position = 0;
        }
        using MemoryStream ms = new MemoryStream();
        input.CopyTo(ms);
        return ms.ToArray();
    }
}