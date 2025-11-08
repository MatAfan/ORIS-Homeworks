using System.Net;
using System.Text;
using System.Text.Json;

namespace MiniHttpServer;
using MiniHttpServer.share;

internal class Program
{
    static void Main(string[] args)
    {
        Start();
    }

    static void Start()
    {
        try // Проверка на корректность настроек
        {
            var settings = SettingsManager.Instance;
        }
        catch (Exception ex)
        {
            Logger.Print($"Ошибка : {ex.Message}");
            // Не запускаем сервер
            return;
        }

        var httpServer = new HttpServer();
        httpServer.StartAsync();

        var stopCommand = "";
        while (!httpServer.ServerIsStop)
        {
            stopCommand = Console.ReadLine();
            if (stopCommand == "/stop")
                httpServer.ServerIsStop = true;
        }

        httpServer.Stop();
    }
}
