using MiniHttpServer.Framework.share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Framework.Core.Abstract;

public class PageResult : IResponseResult
{
    private readonly string _pathTemplate;
    private readonly Dictionary<string, object> _data;

    public PageResult(string pathTemplate, Dictionary<string, object> data)
    {
        _data = data;
        _pathTemplate = pathTemplate;
    }

    public string Execute(HttpListenerContext context)
    {
        // Вызов метода шаблонизатора [+]
        var templateRenderer = new HtmlTemplateRenderer();
        string page = templateRenderer.RenderFromFile(_pathTemplate, _data);

        // реализовать JsonResult
        // Создать проект с тестами для MiniHttpServer.Framework
        // написать тесты для класса HttpServer

        return page;
    }
}
