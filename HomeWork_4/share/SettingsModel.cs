using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MiniHttpServer.share;

public class SettingsManager
{
    private static SettingsManager _Instance;
    private static readonly object _lock = new object();
    public AppSettings Settings { get; private set; }

    private SettingsManager()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        var settingPath = @".\settings.json";
        if (!File.Exists(settingPath))
            throw new FileNotFoundException("Файл настроек не найден.");

        Settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(settingPath))
            ?? throw new InvalidOperationException("Не удалось десериализовать настройки.");

        var validationContext = new ValidationContext(Settings);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(Settings, validationContext, validationResults, true))
        {
            foreach (ValidationResult result in validationResults)
            {
                if (result.ErrorMessage != string.Empty)
                    throw new ArgumentException(result.ErrorMessage);
            }
                //Logger.Print($"Ошибка : {result.ErrorMessage}");
        }
    }

    public static SettingsManager Instance
    {
        get
        {
            // Двойная проверка блокировки для потокобезопасности
            if (_Instance == null)
            {
                lock (_lock)
                {
                    if (_Instance == null)
                        _Instance = new SettingsManager();
                }
            }
            return _Instance;
        }
    }
}

public class AppSettings
{
    public string StaticDirectoryPath { get; set; }
    [RegularExpression(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$", ErrorMessage = "домен указан неверно.")]
    
    public string Domain { get; set; }
    [RegularExpression(@"^\d{4}$", ErrorMessage = "порт указан неверно.")]
    
    public string Port { get; set; }
    public string SenderEmail { get; set; }
    public string SenderName { get; set; }
    public string SenderPassword { get; set; }
    public string SMPTserver { get; set; }

    [RegularExpression(@"\d{1,3}", ErrorMessage = "Порт SMTP указан неверно.")]
    public int SMTPport { get; set; }
}