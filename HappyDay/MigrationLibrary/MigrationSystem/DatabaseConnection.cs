namespace MigrationSystem;

public class DatabaseConnection(string connectionString)
{
    private string connectionString = connectionString;
    
    private Dictionary<string, Dictionary<string, string>> tables = new();
    
    
}