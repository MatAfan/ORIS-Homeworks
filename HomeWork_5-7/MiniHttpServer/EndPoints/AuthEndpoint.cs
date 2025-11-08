using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Core.Abstract;
using MiniHttpServer.Services;

namespace MiniHttpServer.Endpoints;

[Endpoint]
internal class AuthEndpoint : BaseEndPoint
{
    // Get /auth/
    [HttpGet]
    public string /*IResponseResult*/ LoginPage()
    {
        // сделать
        return "login.html";

        //return Page("template_path", data);
    }

    // Post /auth/
    [HttpPost]
    public void Login(string email, string password)
    {


        // Отправка на почту email указанного email и password
        // EmailService.SendEmail(email, title, message);
    }


    // Post /auth/sendEmail
    [HttpPost("sendEmail")]
    public void SendEmail(string to, string title, string message)
    {
        // Отправка на почту email указанного email и password
        EmailService.SendEmail(to, title, message);
    }

}
