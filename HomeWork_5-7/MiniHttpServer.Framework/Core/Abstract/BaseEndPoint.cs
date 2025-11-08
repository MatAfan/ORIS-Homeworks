using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Framework.Core.Abstract;

abstract public class BaseEndPoint
{
    protected HttpListenerContext Context { get; private set; }

    public void SetContext(HttpListenerContext context)
    {
        Context = context;
    }

    protected IResponseResult Page(string pathTemplate, Dictionary<string, object> data) => new PageResult(pathTemplate, data);
}
