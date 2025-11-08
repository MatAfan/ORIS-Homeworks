using MiniHttpServer.Attributes;
using MiniHttpServer.Core.Abstract;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace MiniHttpServer.Core.Handlers;

internal class EndpointsHandler : Handler
{
    public override async Task HandleRequest(HttpListenerContext context)
    {
        if (true)
        {
            var request = context.Request;
            var requestSplit = request.Url?.AbsolutePath.Split('/');
            var endpointName = requestSplit[requestSplit.Length - 2];

            var assembly = Assembly.GetExecutingAssembly();
            var endpont = assembly.GetTypes()
                                   .Where(t => t.GetCustomAttribute<EndpointAttribute>() != null)
                                   .FirstOrDefault(end => IsCheckedNameEndpoint(end.Name, endpointName));

            if (endpont == null)
            {
                Logger.PrintError("Контроллер не найден.");
                return; // TODO: 
            }

            var method = endpont.GetMethods().Where(t => t.GetCustomAttributes(true)
                        .Any(attr => attr.GetType().Name.Equals($"Http{context.Request.HttpMethod}",
                                                                StringComparison.OrdinalIgnoreCase)))
                        .FirstOrDefault(m => m.Name.ToLower() == requestSplit[requestSplit.Length - 1]);

            if (method == null)
            {
                Logger.PrintError($"Метод контроллера {endpointName} не определён.");
                return;  // TODO:            
            }

            Dictionary<string, string> data;
            using (StreamReader streamReader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                data = ParseFormData(streamReader.ReadToEnd());
            }

            switch (method.Name)
            {
                //< Обработка ошибок >//
                // < Вынести всё в цикл поиска метода по имени , затем поиск аргументов по имени > //
                case "Login":
                    method.Invoke(Activator.CreateInstance(endpont)
                        , new object[] { data["email"], data["password"] });
                    break;

                case "SendEmail":
                    var mesage = 
@"<h1>Давыдов Андрей</h1>
<h2>11-408</h2>";
                    method.Invoke(Activator.CreateInstance(endpont)
                        , new object[3] { data["email"], "Тест", mesage });
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.OutputStream.Close();
                    break;

                case "LoginPage":
                    var page = method.Invoke(Activator.CreateInstance(endpont)
                        , null);
                    var responseBytes = GetResponseBytes.Invoke((string)page);
                    context.Response.ContentLength64 = (long)responseBytes?.Length;
                    context.Response.OutputStream.Write(responseBytes);
                    break;

                    default:
                    break;
            }

            Console.WriteLine($"Метод -{method.Name}- выполнен!");

        }
        // передача запроса дальше по цепи при наличии в ней обработчиков
        else if (Successor != null)
        {
            Successor.HandleRequest(context);
        }
    }

    private bool IsCheckedNameEndpoint(string endpointName, string className) =>
        endpointName.Equals(className, StringComparison.OrdinalIgnoreCase) ||
        endpointName.Equals($"{className}Endpoint", StringComparison.OrdinalIgnoreCase);

    private static Dictionary<string, string> ParseFormData(string formData)
    {
        var result = new Dictionary<string, string>();
        var pairs = formData.Split('&');

        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=');
            if (keyValue.Length == 2)
            {
                string key = Uri.UnescapeDataString(keyValue[0]);
                string value = Uri.UnescapeDataString(keyValue[1]);
                result[key] = value;
            }
        }

        return result;
    }
}
