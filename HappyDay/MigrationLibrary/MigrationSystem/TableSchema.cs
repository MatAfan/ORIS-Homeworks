namespace MigrationSystem;

public class TableSchema
{
    public string TableName { get; set; } = string.Empty;
    public List<ColumnDefinition> Columns { get; set; } = new List<ColumnDefinition>();
}

public class ColumnDefinition
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    private bool isPrimaryKey;
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