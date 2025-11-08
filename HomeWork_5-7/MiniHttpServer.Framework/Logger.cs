using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Framework;

public static class Logger
{
    public static void Print(string message)
    {
        Console.WriteLine(message);
    }

    public static void PrintError(string message)
    {
        Console.BackgroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
