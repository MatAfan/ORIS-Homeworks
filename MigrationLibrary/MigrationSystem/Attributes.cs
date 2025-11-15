namespace MigrationSystem;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class Table(string name) : Attribute { public readonly string Name = name; }

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class Column : Attribute;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class PrimaryKey : Attribute;
