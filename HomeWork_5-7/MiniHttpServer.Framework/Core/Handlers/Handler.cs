using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Framework.Core.Handlers;

abstract public class Handler
{
    public Handler Successor { get; set; }
    public abstract Task HandleRequest(HttpListenerContext context);
}
