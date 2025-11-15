using System.Net;
using System.Text;

namespace ServerAPI;
 
public class HttpServer
{
    private readonly HttpListener _listener = new();
    private readonly Dictionary<string, Dictionary<string, Action<HttpListenerContext>>> _routes = new();
    private bool _isRunning;

    public void AddRoute(string method, string route, Action<HttpListenerContext> handler)
    {
        method = method.ToUpper();
        if (!_routes.ContainsKey(method))
            _routes[method] = new Dictionary<string, Action<HttpListenerContext>>();
        _routes[method][route] = handler;
    }

    public void Start(string prefix)
    {
        _listener.Prefixes.Add(prefix);
        _listener.Start();
        _isRunning = true;

        Console.WriteLine($"Server started at {prefix}");
        Task.Run(Listen);
    }

    public void Stop()
    {
        _isRunning = false;
        _listener.Stop();
        _listener.Close();
    }

    private async Task Listen()
    {
        while (_isRunning)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequest(context));  // Process requests in parallel
            }
            catch (HttpListenerException)
            {
                // Listener was stopped
                break;
            }
        }
    }

    private void HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        string method = request.HttpMethod;
        string path = request.Url.AbsolutePath;

        // Find matching route
        if (_routes.ContainsKey(method) && _routes[method].ContainsKey(path))
        {
            _routes[method][path](context);
        }
        else
        {
            // Route not found
            response.StatusCode = (int)HttpStatusCode.NotFound;
            byte[] buffer = Encoding.UTF8.GetBytes("404 - Not Found");
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }
    }
}