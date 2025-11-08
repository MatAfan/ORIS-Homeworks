using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiniHttpServer.Framework.share;

public class HtmlTemplateRenderer : IHtmlTemplateRenderer
{
    private Dictionary<string, object> Objects;

    public string RenderFromFile(string filePath, Dictionary<string,object> dataModel)
    {
        Objects = dataModel;
        var html = File.ReadAllText(filePath);
        var template = RenderFromString(html, Objects);
        return template;
    }

    public string RenderFromString(string htmlTemplate, Dictionary<string, object> dataModel)
    {
        Objects = dataModel; 
        var htmlRender = Render(htmlTemplate);
        return htmlRender;
    }

    public string RenderToFile(string inputFilePath, string outputFilePath, Dictionary<string, object> dataModel)
    {
        Objects = dataModel;
        var html = File.ReadAllText(inputFilePath);
        var template = RenderFromString(html, Objects);
        File.WriteAllText(outputFilePath, template);
        return template;
    }

#region RENDER
    private string Render(string htmlTemplate)
    {
        var strB = new StringBuilder();
        var index = 0;
        while (index < htmlTemplate.Length)
        {
            if (htmlTemplate[index] != '$')
            {
                strB.Append(htmlTemplate[index]);
                index++;
            }
            else
            {
                Regex regex = null;
                Match match = null;
                switch (htmlTemplate[index+1])
                {
                    case '{':
                        regex = new Regex(@"\${(?<Prop>[^}]+)}");
                        match = regex.Match(htmlTemplate, index);
                        strB.Append(PropRender(match));
                        index += match.Length;
                        break;

                    case 'f': // Не вложенный форич
                        regex = new Regex(@"\$foreach\(var (?<Item>.+) in (?<Collection>.+)\)\r*\n*(?<Content>[\D0-9]*?)\r*\n*\$endfor");
                        match = regex.Match(htmlTemplate, index);
                        strB.Append(ForeachRender(match));
                        index += match.Length;
                        break;

                    case 'i': // Не вложенный иф
                        regex = new Regex(@"\$if\((?<Statement>.+)\)\r*\n*(?<True>[\D0-9]+?)\r*\n*(?:\$else\r*\n*(?<False>[\D0-9]+?))*\r*\n*\$endif");
                        match = regex.Match(htmlTemplate, index);
                        strB.Append(IfRender(match));
                        index += match.Length;
                        break;

                    default:
                        break;
                }
            }
        }
        return strB.ToString();
    }

    private string IfRender(Match htmlPart)
    {
        var strB = new StringBuilder();
        var statementGroup = htmlPart.Groups["Statement"].Value.Split(".");
        var objName = statementGroup[0];

        var statement = GetObjectByReflection(statementGroup);

        if (statement == null)
            return htmlPart.Value;

        var hasElse = htmlPart.Groups.ContainsKey("False");

        if ((bool)statement)
        {
            strB.Append(Render(htmlPart.Groups["True"].Value));
        }
        else
        {
            if (hasElse)
            {
                strB.Append(Render(htmlPart.Groups["False"].Value));
            }
        }

        return strB.ToString();
    }

    private string ForeachRender(Match htmlPart)
    {
        var strB = new StringBuilder();
        var collectionName = htmlPart.Groups["Collection"].Value.Split(".");
        // get collection
        if (!Objects.ContainsKey(collectionName[0]))
            return htmlPart.Value;
        var collection = GetObjectByReflection(collectionName);

        if (collection == null)
            return htmlPart.Value;

        var itemName = htmlPart.Groups["Item"].Value;
        Objects.Add(itemName, null); // add
        if (collection is IEnumerable c)
        {
            foreach (var item in c)
            {
                Objects[itemName] = item; // update
                // DO RENDER
                strB.Append(Render(htmlPart.Groups["Content"].Value));
                strB.Append("\r\n");
            }
            strB.Remove(strB.Length-2,2);
            Objects.Remove(itemName); // delete
        }

        return strB.ToString();
    }

    private string PropRender(Match htmlPart)
    {
        // подаётся регекс
        var propGroup = htmlPart.Groups["Prop"].Value.Split('.');
        // ищем объект в словаре (exсeptions)
        if (!Objects.ContainsKey(propGroup[0]))
            return htmlPart.Value;

        var obj = GetObjectByReflection(propGroup);

        if (obj == null)
            return htmlPart.Value;

        return obj.ToString();
    }

    private object GetObjectByReflection(string[] propGroup)
    {
        if (!Objects.ContainsKey(propGroup[0]))
            return null; // Не нашел => null

        var obj = Objects[propGroup[0]];
        // ищем нужное свойство
        if (propGroup.Length == 1)
        {
            return obj;
        }

        try
        {
            var prop = new object();
            for (int i = 1; i < propGroup.Length; i++)
            {
                var objType = obj.GetType();
                prop = objType.GetProperty(propGroup[i]).GetValue(obj);
                obj = prop;
            }
        }
        catch (Exception nullEx)
        {
            return null;
        }
        return obj;
    }
#endregion

}

public class User
{
    public string Name { get; set; }
    public Passport Passport { get; set; }
    public bool Hyligan { get; set; }
    public List<int> List { get; set; }
}

public class Passport
{
    public int Code { get; set; }
}

public interface IHtmlTemplateRenderer
{
    string RenderFromString(string htmlTemplate, Dictionary<string, object> dataModel);

    string RenderFromFile(string filePath, Dictionary<string, object> dataModel);

    string RenderToFile(string inputFilePath, string outputFilePath, Dictionary<string, object> dataModel);
};