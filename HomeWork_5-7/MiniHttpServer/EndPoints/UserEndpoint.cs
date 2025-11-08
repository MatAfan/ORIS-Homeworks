using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Core.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.EndPoints;

public class UserEndpoint : BaseEndPoint
{
    [HttpGet("users")]
    public IResponseResult GetUsers()
    {
        return null;
    }
}
