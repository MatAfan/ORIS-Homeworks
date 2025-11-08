using System.Net;
using System.Text;
using System.Text.Json;

namespace MiniHttpServer;
using MiniHttpServer.share;
/*
    Добавить обработку исключений
 */

internal class Program
{
    static void Main(string[] args)
    {
        Start();
    }

    static void Start()
    {
        var settingsModel = SettingsModel.GetInstance();

        if (settingsModel == null)
        {
            Logger.Print("Ошибка : проблема с получением модели настройки.");
            return;
        }

        var httpServer = new HttpServer(settingsModel);
        httpServer.StartAsync();

        var stopCommand = "";
        while (!settingsModel.IsStop)
        {
            stopCommand = Console.ReadLine();
            if (stopCommand == "/stop")
                settingsModel.IsStop = true;
        }

        httpServer.Stop();
    }
}
