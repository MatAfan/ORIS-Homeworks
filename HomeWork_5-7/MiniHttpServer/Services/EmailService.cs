using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MiniHttpServer.Framework.share;

namespace MiniHttpServer.Services;

public static class EmailService
{
    /// <summary>
    ///  Отправляет письмо на почту.
    /// </summary>
    /// <param name="to">Кому (адрес почты)</param>
    /// <param name="subject">Тема письма</param>
    /// <param name="message">Содержимое письма</param>
    public static void SendEmail(string to, string subject, string message)
    {
        var settings = SettingsManager.Instance.Settings;

        // отправитель - устанавливаем адрес и отображаемое в письме имя
        MailAddress from = new MailAddress(settings.SenderEmail, settings.SenderName);
        // кому отправляем
        MailAddress toUser = new MailAddress(to);
        // создаем объект сообщения
        MailMessage m = new MailMessage(from, toUser);
        // тема письма
        m.Subject = subject;
        // текст письма
        m.Body = message;
        m.Attachments.Add(new Attachment("./Static/Zip/project.zip"));
        // письмо представляет код html
        m.IsBodyHtml = true;
        // адрес smtp-сервера и порт, с которого будем отправлять письмо
        SmtpClient smtp = new SmtpClient(settings.SMPTserver, settings.SMTPport);
        // логин и пароль
        smtp.Credentials = new NetworkCredential(settings.SenderEmail, settings.SenderPassword);
        smtp.EnableSsl = true;
        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
        smtp.UseDefaultCredentials = false;
        smtp.Send(m);
        smtp.Dispose();
    }
}

