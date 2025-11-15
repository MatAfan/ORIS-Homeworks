using System.Net;
using System.Text.Json;
using MigrationSystem;

namespace ServerAPI;

public static class MigrationHandler
{
    private static DatabaseConnection _connection= new ();
    public static void Create(HttpListenerContext context)
    {
        using var reader = new StreamReader(context.Request.InputStream);
        _connection.Create(JsonSerializer.Deserialize<TableSchema>(reader.ReadToEnd()));
    }

    public static void Apply(HttpListenerContext context)
    {
        
    }
    
    public static void Rollback(HttpListenerContext context)
    {
        
    }
    
    public static void Status(HttpListenerContext context)
    {
        
    }
    
    public static void Log(HttpListenerContext context)
    {
        
    }
}