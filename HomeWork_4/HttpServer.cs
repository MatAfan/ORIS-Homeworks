using MiniHttpServer.Core.Handlers;
using MiniHttpServer.share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer;

public class HttpServer
{
    private readonly SettingsManager _SettingsModel;
    private readonly HttpListener _HttpListener;
    public bool ServerIsStop = true;

    public HttpServer()
    {
        _SettingsModel = SettingsManager.Instance;
        _HttpListener = new HttpListener();
        ServerIsStop = false;
    }

    public async Task StartAsync()
    {
        _HttpListener.Prefixes.Add($"http://{_SettingsModel.Settings.Domain}:{_SettingsModel.Settings.Port}/");
        _HttpListener.Start();
        Logger.Print("Сервер запущен.\nОжидаем запрос.");
        await Receive();
    }

    public void Stop()
    {
        _HttpListener.Stop();
        _HttpListener.Close();
        Logger.Print("Сервер закрыт.");
    }

    private async Task Receive()
    {
        while (_HttpListener.IsListening && !ServerIsStop)
        {
            try
            {
                var context = await _HttpListener.GetContextAsync();

            #region HANDLERS
                Console.WriteLine("======================================================"); ;
                Logger.Print("Запрос получен.");

                Handler staticFilesHandler = new StaticFilesHandler();
                Handler endpointsHandler = new EndpointsHandler();
                staticFilesHandler.Successor = endpointsHandler;
                await staticFilesHandler.HandleRequest(context);
             #endregion
            }
            catch (HttpListenerException le)
            {
                Logger.Print($"HTTP Listener Error: {le.Message}.");
                break;
            }
            catch (Exception ex)
            {
                Logger.Print($"Ошибка : {ex}.");
                break;
            }
        }
    }
}
