using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Core.Abstract;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace MiniHttpServer.Framework.Core.Handlers;

internal class EndpointsHandler : Handler
{
    public override async Task HandleRequest(HttpListenerContext context)
    {

        var request = context.Request;
        var requestSplit = request.Url?.AbsolutePath.Split('/');
        var endpointName = requestSplit[requestSplit.Length - 2];

        var assembly = Assembly.GetEntryAssembly();
        var endpoint = assembly.GetTypes()
                                .Where(t => t.GetCustomAttribute<EndpointAttribute>() != null)
                                .FirstOrDefault(end => IsCheckedNameEndpoint(end.Name, endpointName));

        if (endpoint == null)
        {
            Logger.PrintError("Контроллер не найден.");
            return; // TODO: 
        }

        bool isBaseEndpoint = assembly.GetTypes()
            .Any(t => typeof(BaseEndPoint).IsAssignableFrom(t) && !t.IsAbstract);

        var instanceEndpoint = Activator.CreateInstance(endpoint);

        if (isBaseEndpoint)
        {
            (instanceEndpoint as BaseEndPoint).SetContext(context);
        }


        var method = endpoint.GetMethods().Where(t => t.GetCustomAttributes(true)
                    .Any(attr => attr.GetType().Name.Equals($"Http{context.Request.HttpMethod}",
                                                            StringComparison.OrdinalIgnoreCase)))
                    .FirstOrDefault(m => m.Name.ToLower() == requestSplit[requestSplit.Length - 1]);

        if (method == null)
        {
            Logger.PrintError($"Метод контроллера {endpointName} не определён.");
            return;  // TODO:            
        }

        //<Добавлять данные в словарь и удалять/обновлять по необходимости>//
        Dictionary<string, string> data;
        using (StreamReader streamReader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
        {
            data = ParseFormData(streamReader.ReadToEnd());
        }

        /*switch (method.Name)
        {
            //< Обработка ошибок >//
            // < Вынести всё в цикл поиска метода по имени , затем поиск аргументов по имени > //
            case "Login":
                method.Invoke(instanceEndpoint
                    , new object[] { data["email"], data["password"] });
                break;

            case "SendEmail":
                var mesage = 
@"<h1>Давыдов Андрей</h1>
<h2>11-408</h2>";
                method.Invoke(instanceEndpoint
                    , new object[3] { data["email"], "Тест", mesage });
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Close();
                break;

            case "LoginPage":
                var page = method.Invoke(instanceEndpoint
                    , null);
                var responseBytes = GetResponseBytes.Invoke((string)page);
                context.Response.ContentLength64 = (long)responseBytes?.Length;
                context.Response.OutputStream.Write(responseBytes);
                break;

                default:
                break;
        }*/

        var param = method.GetParameters();
        var objects = new List<object>();
        foreach (var paramInfo in param)
        {
            if (data.ContainsKey(paramInfo.Name))
                objects.Add(data[paramInfo.Name]);
        }
        if (objects.Count == 0)
            objects = null;
        //<Проверка: все ли аргументы в наличие>//
        var result = method.Invoke(instanceEndpoint, objects?.ToArray());
        if (result is string str)
        {
            context.Response.ContentType = "text/plain";
            SendResponse(context.Response, str);
        }
        if (result is PageResult pageResult)
        {
            context.Response.ContentType = "text/html";
            SendResponse(context.Response, pageResult.Execute(context));
        }

        Console.WriteLine($"Метод -{method.Name}- выполнен!");

        // передача запроса дальше по цепи при наличии в ней обработчиков
        if (Successor != null)
        {
            Successor.HandleRequest(context);
        }
    }

    private void SendResponse(HttpListenerResponse response, string str)
    {
        response.StatusCode = (int)HttpStatusCode.OK;
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        response.ContentLength64 = bytes.Length;
        response.OutputStream.Write(bytes);
        response.Close();
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
