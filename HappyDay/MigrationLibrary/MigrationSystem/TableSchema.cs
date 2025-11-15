namespace MigrationSystem;

public class TableSchema
{
    public string TableName = string.Empty;
    public List<ColumnDefinition> Columns = new();

    public string FindPrimaryKey() =>
        Columns.Where(x => x.IsPrimaryKey != null && x.IsPrimaryKey.Value)
               .Select(x => x.ColumnName)
               .FirstOrDefault(defaultValue: "Id");

    public (HashSet<ColumnDefinition> Remove, HashSet<ColumnDefinition> Add)? FindDifference(TableSchema table)
    {
        if (table.TableName != TableName) return null;
        var Remove = new HashSet<ColumnDefinition>();
        var Add = new HashSet<ColumnDefinition>();
        foreach (var column in table.Columns) if (!Columns.Contains(column)) Add.Add(column);
        foreach (var column in Columns) if (!table.Columns.Contains(column)) Remove.Add(column);
        return (Remove, Add);
    }
}

public struct ColumnDefinition
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    private bool isPrimaryKey;

    public ColumnDefinition()
    {
        isPrimaryKey = false;
    }

    public bool? IsPrimaryKey
    {
        get { return isPrimaryKey; }
        set
        {
            if (value == null) isPrimaryKey = false;
            else isPrimaryKey = value.Value;
        } 
    }
}