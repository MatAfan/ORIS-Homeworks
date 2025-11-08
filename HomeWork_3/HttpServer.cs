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
    private readonly Dictionary<string, string> _ContentExtensions = new Dictionary<string, string>()
    {
        // Изображения
        {"png" , "image/png" },
        {"jpg" , "image/jpeg" },
        {"jpeg" , "image/jpeg"},
        {"gif" , "image/gif" },
        {"bmp" , "image/bmp" },
        {"webp" , "image/webp" },
        {"svg" , "image/svg+xml" },
        {"ico" , "image/x-icon" },
        
        // Документы
        {"pdf" , "application/pdf" },
        {"doc" , "application/msword" },
        {"docx" , "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        {"xls" , "application/vnd.ms-excel" },
        {"xlsx" , "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        {"ppt" , "application/vnd.ms-powerpoint" },
        {"pptx" , "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
        
        // Текстовые файлы
        {"txt" , "text/plain" },
        {"html" , "text/html" },
        {"htm" , "text/html" },
        {"css" , "text/css" },
        {"js" , "application/javascript" },
        {"json" , "application/json" },
        {"xml" , "application/xml" },
        
        // Архивы
        {"zip" , "application/zip" },
        {"rar" , "application/x-rar-compressed" },
        {"7z" , "application/x-7z-compressed" },
        {"tar" , "application/x-tar" },
        
        // Аудио
        {"mp3" , "audio/mpeg" },
        {"wav" , "audio/wav" },
        {"ogg" , "audio/ogg" },
        
        // Видео
        {"mp4" , "video/mp4" },
        {"avi" , "video/x-msvideo" },
        {"mov" , "video/quicktime" },
        {"webm" , "video/webm" },
    };
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
                Console.Clear();
                Logger.Print("Запрос получен.");

                Task.Run(async () =>
                {
                    await SendResponse(context);
                });
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

    private async Task SendResponse(HttpListenerContext context)
    {
        var response = context.Response;
        var path = context.Request.Url.AbsolutePath;
        if (path == "/")
            path = _SettingsModel.Settings.StaticDirectoryPath;
        else
            path = "." + path;

        try
        {
            // content type
            var fileInfo = new FileInfo(path);
            response.ContentType = _ContentExtensions[fileInfo.Extension.Substring(1)];

            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                response.ContentLength64 = fileStream.Length;
                fileStream.CopyTo(response.OutputStream);
            }

            Logger.Print("Запрос обработан.");
        }
        catch (FileNotFoundException)
        {
            Logger.Print("Ошибка : файл не найден.");
            response.StatusCode = 404;
            Console.Beep();
        }
        catch (KeyNotFoundException)
        {
            Logger.Print("Ошибка : расширение для файла не добавлено в словарь.");
        }
        finally
        {
            response.Close();
        }
    }
}
