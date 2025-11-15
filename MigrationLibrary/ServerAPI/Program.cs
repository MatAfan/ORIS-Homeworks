using ServerAPI;
using static ServerAPI.MigrationHandler;

var server = new HttpServer();
        
server.AddRoute("POST", "/migration/create", Create);
server.AddRoute("POST", "/migration/apply", Apply);
server.AddRoute("POST", "/migration/rollback", Rollback);
server.AddRoute("GET", "/migration/status", Status);
server.AddRoute("GET", "/migration/log", Log);

server.Start("http://localhost:8080/");

Console.WriteLine("Press any key to stop the server...");
Console.ReadKey();
server.Stop();
