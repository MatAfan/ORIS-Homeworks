using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.share;

public class ContentExtension
{
    private static readonly Dictionary<string, string> _ContentExtensions = new Dictionary<string, string>()
    {
        // Изображения
        {".png" , "image/png" },
        {".jpg" , "image/jpeg" },
        {".jpeg" , "image/jpeg"},
        {".gif" , "image/gif" },
        {".bmp" , "image/bmp" },
        {".webp" , "image/webp" },
        {".svg" , "image/svg+xml" },
        {".ico" , "image/x-icon" },
        
        // Документы
        {".pdf" , "application/pdf" },
        {".doc" , "application/msword" },
        {".docx" , "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        {".xls" , "application/vnd.ms-excel" },
        {".xlsx" , "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        {".ppt" , "application/vnd.ms-powerpoint" },
        {".pptx" , "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
        
        // Текстовые файлы
        {".txt" , "text/plain" },
        {".html" , "text/html" },
        {".htm" , "text/html" },
        {".css" , "text/css" },
        {".js" , "application/javascript" },
        {".json" , "application/json" },
        {".xml" , "application/xml" },
        
        // Архивы
        {".zip" , "application/zip" },
        {".rar" , "application/x-rar-compressed" },
        {".7z" , "application/x-7z-compressed" },
        {".tar" , "application/x-tar" },
        
        // Аудио
        {".mp3" , "audio/mpeg" },
        {".wav" , "audio/wav" },
        {".ogg" , "audio/ogg" },
        
        // Видео
        {".mp4" , "video/mp4" },
        {".avi" , "video/x-msvideo" },
        {".mov" , "video/quicktime" },
        {".webm" , "video/webm" },
    };
    public static string GetExtension(string filePath)
    {
        // TODO : Отловить ошибки
        var fileInfo = new FileInfo(filePath);
        return _ContentExtensions[fileInfo.Extension];
    }
}
