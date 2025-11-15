# Usage of this ****
**Use these methods**
 - POST /migrate/create - create a migration without appliance
 - POST /migrate/apply - apply last
 - POST /migrate/rollback - rollback last
 - GET /migrate/status - sends a JSON of a list of tables with their columns and list of all migrations
 - GET /migrate/log - sends a JSON with migration history

Method body is required only for migrate/create - sends a data model in JSON:
```json
{
  "TableName": "name",
  "Columns": 
  [
    {
      "ColumnName": "Id",
      "DataType": "int",
      "IsPrimaryKey": false
    }
  ]
}
```
Columns contains a list of data about columns, IsPrimaryKey parameter is optional
