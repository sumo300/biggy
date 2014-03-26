using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using Biggy.Extensions;
using System.Configuration;
using System.Text.RegularExpressions;
using Inflector;

namespace Biggy
{
  public abstract class DbHost {
    public abstract string DbDelimiterFormatString { get; }
    public abstract DbConnection OpenConnection();
    protected abstract void LoadDbColumnsList();
    protected abstract void LoadDbTableNames();
    public abstract bool TableExists(string delimitedTableName);


    public string ConnectionString { get; set; }
    public List<DbColumnMapping> DbColumnsList { get; set; }
    public List<string> DbTableNames { get; set; }


    public DbHost(string connectionStringName) {
      ConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
      this.LoadSchemaInfo();
    }

    public void LoadSchemaInfo() {
      this.LoadDbTableNames();
      this.LoadDbColumnsList();
    }

    public virtual DBTableMapping getTableMappingFor<T>() where T : new() {
      var result = new DBTableMapping(this.DbDelimiterFormatString);
      var item = new T();
      var itemType = item.GetType();
      var properties = itemType.GetProperties();

      string replaceString = "[^a-zA-Z1-9]";
      var rgx = new Regex(replaceString);

      string flattenedItemTypeName = rgx.Replace(itemType.Name.ToLower(), "");
      string plural = Inflector.Inflector.Pluralize(flattenedItemTypeName);
      var dbTableName = this.DbTableNames.FirstOrDefault(t => rgx.Replace(t.ToLower(), "") == flattenedItemTypeName);
      if(dbTableName == null) {
        dbTableName = this.DbTableNames.FirstOrDefault(t => rgx.Replace(t.ToLower(), "") == plural);
      }
      else
      {
        var tableNameAttribute = itemType.GetCustomAttributes(false).FirstOrDefault(a => a.GetType() == typeof(DbTableAttribute)) as DbTableAttribute;
        if(tableNameAttribute != null)
        {
          dbTableName = tableNameAttribute.Name;
        }
      }

      result.DBTableName = dbTableName;
      result.MappedTypeName = itemType.Name;

      var dbColumnInfo = from c in this.DbColumnsList where c.TableName == dbTableName select c;
      foreach(var property in properties) {
        string flattenedPropertyName = rgx.Replace(property.Name.ToLower(), "");
        DbColumnMapping columnMapping = dbColumnInfo.FirstOrDefault(c => rgx.Replace(c.ColumnName.ToLower(), "") == flattenedPropertyName);
        if(columnMapping != null) {
          columnMapping.PropertyName = property.Name;
          columnMapping.DataType = itemType;
        } else {
          // Look for a custom column name attribute:
          DbColumnAttribute mappedColumnAttribute = null;
          var attribute = property.GetCustomAttributes(false).FirstOrDefault(a => a.GetType() == typeof(DbColumnAttribute));
          if (attribute != null) {
            // Use the column name found in the attribute:
            mappedColumnAttribute = attribute as DbColumnAttribute;
            string matchColumnName = mappedColumnAttribute.Name;
            columnMapping = dbColumnInfo.FirstOrDefault(c => c.ColumnName == matchColumnName);
            columnMapping.PropertyName = property.Name;
            columnMapping.DataType = itemType;
          }
        }
        if (columnMapping != null) {
          result.ColumnMappings.Add(columnMapping);
          if (columnMapping.IsPrimaryKey) {
            result.PrimaryKeyMapping.Add(columnMapping);
          }
        }
      }
      return result;
    }

    public int Execute(DbCommand command) {
      return Execute(new DbCommand[] { command });
    }

    public int Execute(string sql, params object[] args) {
      return Execute(CreateCommand(sql, null, args));
    }

    /// <summary>
    /// Executes a series of DBCommands in a transaction
    /// </summary>
    public int Execute(IEnumerable<DbCommand> commands) {
      var result = 0;
      using (var conn = OpenConnection()) {
        using (var tx = conn.BeginTransaction()) {
          foreach (var cmd in commands) {
            cmd.Connection = conn;
            cmd.Transaction = tx;
            result += cmd.ExecuteNonQuery();
          }
          tx.Commit();
        }
      }
      return result;
    }

    /// <summary>
    /// Creates a DBCommand that you can use for loving your database.
    /// </summary>
    public DbCommand CreateCommand(string sql, DbConnection conn, params object[] args) {
      conn = conn ?? OpenConnection();
      var result = (DbCommand)conn.CreateCommand();
      result.CommandText = sql;
      if (args.Length > 0) {
        result.AddParams(args);
      }
      return result;
    }

    /// <summary>
    /// Returns a single result
    /// </summary>
    public object Scalar(string sql, params object[] args) {
      object result = null;
      using (var conn = OpenConnection()) {
        result = CreateCommand(sql, conn, args).ExecuteScalar();
      }
      return result;
    }

    // TODO: Clean this up and make it consistent with TableExists. Handle the Delimited/not delimited table name mismatch
    public void DropTable(string delimitedTableName) {
      string sql = string.Format("DROP TABLE {0}", delimitedTableName);
      this.Execute(sql);
    }

    // TODO: This can be made mo' bettah by passing in ColumnDef objects to handle delimiting and such:
    public void CreateTable(string delimitedTableName, List<string> columnDefs) {
      string columnDefinitions = string.Join(",", columnDefs.ToArray());
      var sql = string.Format("CREATE TABLE {0} ({1});", delimitedTableName, columnDefinitions);
      this.Execute(sql);
      this.LoadSchemaInfo();
    }

  }
}
