using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Framework.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class EndpointAttribute : Attribute
{
    public EndpointAttribute()
    {
        
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class HttpGet : Attribute
{
    public string? Route { get; }
    public HttpGet()
    {
        
    }

    public HttpGet(string? route)
    {
        Route = route;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class HttpPost : Attribute
{
    public string? Route { get; }
    public HttpPost()
    {

    }

    public HttpPost(string? route)
    {
        Route = route;
    }
}