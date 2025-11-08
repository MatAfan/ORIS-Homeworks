using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MiniHttpServer.share;

public class SettingsModel
{
    public string StaticDirectoryPath { get; set; }
    [RegularExpression(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$", ErrorMessage ="домен указан неверно.")]
    public string Domain { get; set; }
    [RegularExpression(@"^\d{4}$", ErrorMessage ="порт указан неверно.")]
    public string Port { get; set; }
    public bool IsStop { get; set; }

    public static SettingsModel GetInstance()
    {
        var settingPath = @".\settings.json";
        SettingsModel settingsModel;
        try
        {
            settingsModel = JsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(settingPath));

            var validationContext = new ValidationContext(settingsModel);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(settingsModel, validationContext, validationResults, true))
            {
                foreach (ValidationResult result in validationResults)
                    Logger.Print($"Ошибка : {result.ErrorMessage}");
                return null;
            }
        }
        catch (Exception)
        {
            Logger.Print("Ошибка : не найден файл settings.json .");
            return null;
        }

        if (!File.Exists(settingsModel.StaticDirectoryPath))
        {
            Logger.Print("Страница Index.html не найдена.");
            return null;
        }
        settingsModel.IsStop = true;
        return settingsModel;
    }
}