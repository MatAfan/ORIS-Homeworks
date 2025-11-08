using MiniHttpServer.share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer;

public class HttpServer
/*
 * Добавить эксепшены
 */
{
    private readonly SettingsModel _SettingsModel;
    private readonly HttpListener _HttpListener;

    public HttpServer(SettingsModel settingsModel)
    {
        _SettingsModel = settingsModel;
        _HttpListener = new HttpListener();
        _SettingsModel.IsStop = false;
    }

    public async Task StartAsync()
    {
        _HttpListener.Prefixes.Add($"http://{_SettingsModel.Domain}:{_SettingsModel.Port}/connection/");
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
        while (_HttpListener.IsListening && !_SettingsModel.IsStop)
        {
            try
            {
                var context = await _HttpListener.GetContextAsync();
                Logger.Print("Запрос получен.");

                Task.Run(async () =>
                {
                    try
                    {
                        await SendResponse(context);
                    }
                    catch (Exception ex)
                    {
                        Logger.Print($"Ошибка обработки запроса: {ex.Message}");
                        try
                        {
                            context.Response.StatusCode = 500;
                            context.Response.Close();
                        }
                        catch
                        {
                            // Игнорируем ошибки при закрытии ответа
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Print($"Ошибка : Сервер закрыт.");
                break;
            }
        }
    }

    private async Task SendResponse(HttpListenerContext context)
    {
        var response = context.Response;
        response.ContentType = "text/html";

        try
        {
            string responseText = File.ReadAllText(_SettingsModel.StaticDirectoryPath);
            byte[] buffer = Encoding.UTF8.GetBytes(responseText);
            // получаем поток ответа и пишем в него ответ
            response.ContentLength64 = buffer.Length;
            using Stream output = response.OutputStream;
            // отправляем данные
            await output.WriteAsync(buffer);
            await output.FlushAsync();

            Logger.Print("Запрос обработан.");
        }
        catch (FileNotFoundException)
        {
            Logger.Print("Ошибка : Index.html не найден.");
        }
        finally
        {
            response.Close();
        }
    }
}
