using System.Net;
using Npgsql;

namespace MigrationSystem;

public class DatabaseConnection(string connectionString)
{
    private string _connectionString = connectionString;
    private NpgsqlConnection connection =  new(connectionString);
    
    private Dictionary<string, TableSchema> tables = new();

    private string[] allowedDataTypes = ["int", "varchar", "text"];
    
    // I assume that database is postgres and schema is default
    
    public void Create(TableSchema table)
    {
        var res = new MigrationEntry();
        if (!tables.ContainsKey(table.TableName))
        {
            tables.Add(table.TableName, table);
            var sql = $"CREATE TABLE {table.TableName} (\n";
            foreach (var column in table.Columns)
            {
                sql +=
                    $"{column.ColumnName} {(allowedDataTypes.Contains(column.DataType) ? column.DataType : "text")},\n";
            }

            sql += ");";
            res.name = table.TableName;
            res.upsql = sql;
            res.downsql = $"DROP TABLE {table.TableName};";
        }
        else
        {
            var dif = tables[table.TableName].FindDifference(table);
            if (dif != null)
            {
                var sql = "";
                var backsql = "";
                foreach (var column in dif.Value.Add)
                {
                    sql +=
                        $"ALTER TABLE {table.TableName} ADD COLUMN {column.ColumnName} {(allowedDataTypes.Contains(column.DataType) ? column.DataType : "text")};\n";
                    backsql +=
                        $"ALTER TABLE {table.TableName} DROP COLUMN {column.ColumnName} {(allowedDataTypes.Contains(column.DataType) ? column.DataType : "text")};\n";
                }
                foreach (var column in dif.Value.Remove)
                {
                    sql +=
                        $"ALTER TABLE {table.TableName} DROP COLUMN {column.ColumnName} {(allowedDataTypes.Contains(column.DataType) ? column.DataType : "text")};\n";
                    backsql +=
                        $"ALTER TABLE {table.TableName} ADD COLUMN {column.ColumnName} {(allowedDataTypes.Contains(column.DataType) ? column.DataType : "text")};\n";
                }
                res.name = table.TableName;
                res.upsql = sql;
                res.downsql = backsql;
            }
        }

        if (res.name != table.TableName) return;
        _ = ExecuteNonQuery($"INSERT INTO migrations (name, up_sql, down_sql) VALUES ({res.name}, {res.upsql}, {res.downsql});");
    }

    public void Apply()
    {
        var upsql = GetLatestUpSqlAsync("migrations").Result;
        ExecuteNonQuery(upsql);
    }

    public async Task<string?> GetLatestUpSqlAsync(string tableName, string datetimeColumnName = "created_at")
    {
        await connection.OpenAsync();

        // Get the up_sql value from the row with the latest datetime
        using var command = new NpgsqlCommand($@"
        SELECT up_sql 
        FROM {tableName} 
        ORDER BY {datetimeColumnName} DESC 
        LIMIT 1", connection);

        var result = await command.ExecuteScalarAsync();
        return result?.ToString();
    }

    private async Task ExecuteNonQuery(string sql)
    {
        
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
        
    }
}