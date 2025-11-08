using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Framework;

public class GetResponseBytes
{
    public static byte[]? Invoke(string path)
    {

        if (Path.HasExtension(path))
            return TryGetFile(path);
        else
            return TryGetFile(path + "/index.html");
    }

    private static byte[]? TryGetFile(string path)
    {
        try
        {
            var targetPath = Path.Combine(path.Split("/"));
            targetPath = Uri.UnescapeDataString(targetPath);

            var fn = Path.GetFileName(targetPath);
            var ef = Directory.EnumerateFiles("Static", fn, SearchOption.AllDirectories);

            string? found = Directory.EnumerateFiles("Static", $"{Path.GetFileName(path)}", SearchOption.AllDirectories)
                                 .FirstOrDefault(f => f.EndsWith(targetPath, StringComparison.OrdinalIgnoreCase));

            if (found == null)
                throw new FileNotFoundException(path);

            return File.ReadAllBytes(found);
        }
        catch (DirectoryNotFoundException)
        {
            Logger.PrintError("Директория не найдена");
            return null;
        }
        catch (FileNotFoundException)
        {
            if (path != "/favicon.ico")
                Logger.PrintError("Файл не найден");
            return null;
        }
        catch (Exception)
        {
            Logger.PrintError("Ошибка при извлечении текста");
            return null;
        }
    }
}
