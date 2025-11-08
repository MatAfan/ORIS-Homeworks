using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace MyORMLibrary;

public class ORMContext
{
    private readonly string _connectionString;
    public bool IsConnected { get; private set; } = false;

    public ORMContext(string connectionString)
    {
        _connectionString = connectionString;

        using (SqlConnection sqlConnection = new SqlConnection(_connectionString))
        {
            sqlConnection.Open();
            if (sqlConnection.State == ConnectionState.Open)
            {
                IsConnected = true;
            }
            sqlConnection.Close();
        }
    }

    public void AddToTable<T>(T entity, string tableName) where T : class
    {
        // Пример реализации метода Create
        // Параметризованный SQL-запрос для вставки данных
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            if (!TableExists(connection, tableName))
                throw new Exception("Таблица не существует.");

            //<Рефлексией пройтись по свойствам и добавить в параметры запроса>//
            var props = entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            List<string> columns = new List<string>();
            List<string> param = new List<string>();
            List<object> value = new List<object>();

            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                    continue;
                columns.Add(prop.Name);
                param.Add($"@{prop.Name}");
                value.Add(prop.GetValue(entity));
            }
            string sql = $"INSERT INTO {tableName} ({string.Join(",", columns)})\r\nVALUES ({string.Join(",", param)})";
            SqlCommand command = new SqlCommand(sql, connection);

            for (int i = 0; i < param.Count; i++)
            {
                command.Parameters.AddWithValue(param[i], value[i]);
            }
            command.ExecuteNonQuery();
        }

        //throw new NotImplementedException();
    }

    public void CreateTable(Type type, string tableName)
    {
        using (SqlConnection sqlConnection = new SqlConnection(_connectionString))
        {
            sqlConnection.Open();

            if (TableExists(sqlConnection, tableName))
                return;

            var query = new StringBuilder();

            query.Append($"CREATE TABLE {tableName} (\n");
            query.Append("Id INTEGER PRIMARY KEY IDENTITY,\n");

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                query.Append($"{property.Name} {GetColumnDefinition(property)}");

                // Добавляем запятую для всех кроме последней колонки
                if (i < properties.Length - 1)
                    query.Append(",");
                query.Append("\n");
            }

            query.Append(")");

            var command = new SqlCommand(query.ToString(), sqlConnection);
            command.ExecuteNonQuery();
        }
    }

    public string GetColumnDefinition(PropertyInfo property)
    {
        return property.PropertyType switch
        {
            Type t when t == typeof(int) => "INTEGER",
            Type t when t == typeof(string) => "NVARCHAR(MAX)", // упростил
            Type t when t == typeof(bool) => "BIT",
            Type t when t == typeof(DateTime) => "DATETIME2",
            Type t when t == typeof(decimal) => "DECIMAL(18,2)",
            Type t when t == typeof(double) => "FLOAT",
            Type t when t == typeof(float) => "REAL",
            Type t when t == typeof(long) => "BIGINT",
            Type t when t == typeof(short) => "SMALLINT",
            Type t when t == typeof(byte) => "TINYINT",
            Type t when t == typeof(Guid) => "UNIQUEIDENTIFIER",
            Type t when t == typeof(byte[]) => "VARBINARY(MAX)",
            _ => "NVARCHAR(MAX)"
        };
    }

    // Метод проверки существования таблицы
    public bool TableExists(SqlConnection connection, string tableName)
    {
        string checkTableSql = @"
        SELECT CASE 
            WHEN EXISTS (
                SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = @TableName
            ) THEN 1 
            ELSE 0 
        END";

        using (SqlCommand command = new SqlCommand(checkTableSql, connection))
        {
            command.Parameters.AddWithValue("@TableName", tableName);
            return (int)command.ExecuteScalar() == 1;
        }
    }

    public T ReadById<T>(int id, string tableName) where T : class
    {
        T entity = null;
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string sql = $"SELECT * FROM {tableName} WHERE Id = @id";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    entity = DataDeserializer<T>(reader);
                }
            }
        }
        return entity;
    }

    private static T DataDeserializer<T>(SqlDataReader reader) where T : class
    { //<Обработать ошибки>//
        T entity;

        // Маппинг данных из таблицы в объект
        var data = new Dictionary<string, object>();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            string columnName = reader.GetName(i);
            object value = reader.GetValue(i);

            // Преобразование DBNull в null
            if (value == DBNull.Value)
                value = null;

            data[columnName] = value;
        }

        var json = System.Text.Json.JsonSerializer.Serialize(data,
            new JsonSerializerOptions { WriteIndented = true });

        entity = JsonSerializer.Deserialize<T>(json);
        return entity;
    }

    public IEnumerable<T> ReadByAll<T>(string tableName) where T : class
    {
        List<T> entities = new List<T>();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string sql = $"SELECT * FROM {tableName}";
            SqlCommand command = new SqlCommand(sql, connection);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var entity = DataDeserializer<T>(reader);
                    entities.Add(entity);
                }
            }
        }
        return entities;
    }

    public void Update<T>(int id, T entity, string tableName)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            var props = entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            string sql = $"UPDATE {tableName} SET ";
            for (int i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                    continue;
                sql += $"{prop.Name} = @{prop.Name}";
                if (i < props.Length - 1)
                    sql += ", ";
            }
            sql +=" WHERE Id = @id";

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                    continue;
                command.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(entity));
            }

            command.ExecuteNonQuery();
        }
    }

    public void Delete(int id, string tableName)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string sql = $"DELETE FROM {tableName} WHERE Id = @id";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            command.ExecuteNonQuery();
        }
    }

    private T ExecuteQuerySingle<T>(string query) where T : class
    {
        using SqlConnection _connection = new SqlConnection(_connectionString);
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = query;
            _connection.Open();
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return DataDeserializer<T>(reader); // json => entity
                }
            }
            _connection.Close();
        }

        return null;
    }

    private IEnumerable<T> ExecuteQueryMultiple<T>(string query) where T : class
    {
        using SqlConnection _connection = new SqlConnection(_connectionString);
        var results = new List<T>();
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = query;
            _connection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    results.Add(DataDeserializer<T>(reader));
                }
            }
            _connection.Close();
        }
        return results;
    }

    public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class
    {
        var sqlQuery = ExpressionTransformer.BuildSqlQuery(predicate, singleResult: true);
        return ExecuteQuerySingle<T>(sqlQuery);
    }

    public IEnumerable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class
    {
        var sqlQuery = ExpressionTransformer.BuildSqlQuery(predicate, singleResult: false);
        return ExecuteQueryMultiple<T>(sqlQuery);
    }
}

public static class ExpressionTransformer
{
    private static string ParseExpression(Expression expression)
    {
        if (expression is BinaryExpression binary)
        {
            // разбираем выражение на составляющие
            var left = ParseExpression(binary.Left);  // Левая часть выражения
            var right = ParseExpression(binary.Right); // Правая часть выражения
            var op = GetSqlOperator(binary.NodeType);  // Оператор (например, > или =)
            return $"({left} {op} {right})";
        }
        else if (expression is MemberExpression member)
        {
            return member.Member.Name; // Название свойства
        }
        else if (expression is ConstantExpression constant)
        {
            return FormatConstant(constant.Value); // Значение константы
        }

        // TODO: можно расширить для поддержки более сложных выражений (например, методов Contains, StartsWith и т.д.).
        // если не поддерживается то выбрасываем исключение
        throw new NotSupportedException($"Unsupported expression type: {expression.GetType().Name}");
    }

    private static string GetSqlOperator(ExpressionType nodeType)
    {
        return nodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => ">",
            ExpressionType.LessThan => "<",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThanOrEqual => "<=",
            _ => throw new NotSupportedException($"Unsupported node type: {nodeType}")
        };
    }

    private static string FormatConstant(object value)
    {
        return value is string ? $"'{value}'" : value.ToString();
    }

    public static string BuildSqlQuery<T>(Expression<Func<T, bool>> predicate, bool singleResult)
    {
        var tableName = typeof(T).Name + "s"; // Имя таблицы, основанное на имени класса
        var whereClause = ParseExpression(predicate.Body);
        var limitClause = singleResult ? "LIMIT 1" : string.Empty;

        return $"SELECT * FROM {tableName} WHERE {whereClause} {limitClause}".Trim();
    }
}

public class Tour
{
    [NotMapped]
    public int Id { get; set; }
    public string Name { get; set; }
    public int Price { get; set; }

    public override bool Equals(object? obj)
    {
        var flag = true;
        if (obj is Tour other)
        {
            flag = Name == other.Name ? true : false;
            flag = Price == other.Price ? true : false;
        }

        return flag;
    }
}