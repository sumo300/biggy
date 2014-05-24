using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;
using Biggy.Extensions;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace Biggy
{
  public abstract class BiggyRelationalStore<T> : IBiggyStore<T> where T : new() {
    public DbCache Cache { get; set; }

    public abstract DbConnection OpenConnection();
    public abstract string GetInsertReturnValueSQL(string delimitedPkColumn);
    public abstract string GetSingleSelect(string delimitedTableName, string where);
    public abstract string BuildSelect(string where, string orderBy = "", int limit = 0);
    public virtual  string ConnectionString { get { return this.Cache.ConnectionString; } }


    public DBTableMapping tableMapping { get; set; }
    public DbColumnMapping PrimaryKeyMapping { get; set; }

    protected BiggyRelationalStore() { }

    public BiggyRelationalStore(DbCache dbCache) {
      this.Cache = dbCache;
      this.tableMapping = this.getTableMappingForT();

      // Is there a PK? If so, set the member variable:
      if(this.tableMapping.PrimaryKeyMapping.Count == 1) {
        this.PrimaryKeyMapping = this.tableMapping.PrimaryKeyMapping[0];
      }
    }


    public virtual DBTableMapping getTableMappingForT() {
      return this.Cache.getTableMappingFor<T>();
    }


    /// <summary>
    /// Returns all records complying with the passed-in WHERE clause and arguments, 
    /// ordered as specified, limited (TOP) by limit.
    /// </summary>
    public virtual IEnumerable<T> All<T>(string where = "", string orderBy = "", int limit = 0, string columns = "*", params object[] args) where T : new() {
      string sql = this.BuildSelect(where, orderBy, limit);
      var formatted = string.Format(sql, columns, this.tableMapping.DelimitedTableName);
      return Query<T>(formatted, args);
    }

    public virtual T Insert(T item) {
      if(this.BeforeSave(item)) {
        using (var conn = Cache.OpenConnection()) {
          var cmd = (DbCommand)this.CreateInsertCommand(item);
          cmd.CommandText += this.GetInsertReturnValueSQL(this.PrimaryKeyMapping.DelimitedColumnName);
          var newId = cmd.ExecuteScalar();
          if (this.PrimaryKeyMapping != null) {
            this.SetPrimaryKey(item, newId);
          }
        }
        this.Inserted(item);
      }
      return item;
    }

    public virtual int Update(T item) {
      var result = 0;
      if(BeforeSave(item)) {
        using (var conn = Cache.OpenConnection()) {
          var cmd = (DbCommand)CreateUpdateCommand(item);
          result = cmd.ExecuteNonQuery();
        }
      }
      return result;
    }

    /// <summary>
    /// Removes one or more records from the DB according to the passed-in WHERE
    /// </summary>
    public virtual int Delete(T item) {
      var key = this.GetPrimaryKey(item);
      var result = 0;
      if(BeforeDelete(item)) {
        result = this.Execute(CreateDeleteCommand(key: key));
        this.Deleted(item);
      }
      return result;
    }

    /// <summary>
    /// Drops all data from the table - BEWARE
    /// </summary>
    public virtual void DeleteAll()
    {
      this.Execute("DELETE FROM " + this.tableMapping.DelimitedTableName);
    }

    /// <summary>
    /// Drops all data from the table - BEWARE
    /// </summary>
    public virtual int Delete(List<T> items)
    {
      var removed = 0;
      if (items.Count() > 0) {
        //remove from the DB
        var keyList = new List<string>();
        foreach (var item in items) {
          keyList.Add(this.GetPrimaryKey(item).ToString());
        }
        var keySet = String.Join(",", keyList.ToArray());
        var inStatement = this.PrimaryKeyMapping.DelimitedColumnName + " IN (" + keySet + ")";
        removed = this.DeleteWhere(inStatement, "");
      }
      return removed;
    }

    /// <summary>
    /// Removes one or more records from the DB according to the passed-in WHERE
    /// </summary>
    public int DeleteWhere(string where = "", params object[] args) {
      return Execute(CreateDeleteCommand(where: where, args: args));
    }

    /// <summary>
    /// Inserts a large range - does not check for existing entires, and assumes all 
    /// included records are new records. Order of magnitude more performant than standard
    /// Insert method for multiple sequential inserts. 
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public virtual List<T> BulkInsert(List<T> items) {
      int rowsAffected = 0;
      const int MAGIC_PG_PARAMETER_LIMIT = 2100;
      const int MAGIC_PG_ROW_VALUE_LIMIT = 1000;
      var first = items.First();
      string insertClause = "";
      var sbSql = new StringBuilder("");

      using (var conn = Cache.OpenConnection()) {
        using (var transaction = conn.BeginTransaction()) {
          var commands = new List<DbCommand>();
          DbCommand dbCommand = conn.CreateCommand();
          dbCommand.Transaction = transaction;
          var paramCounter = 0;
          var rowValueCounter = 0;

          foreach (var item in items) {
            var itemEx = item.ToExpando();
            var itemSchema = itemEx as IDictionary<string, object>;
            var sbParamGroup = new StringBuilder();
            if (this.PrimaryKeyMapping.IsAutoIncementing) {
              // Don't insert against a serial id:
              string mappedPkPropertyName = this.PrimaryKeyMapping.PropertyName;
              string key = itemSchema.Keys.First(k => k.ToString().Equals(mappedPkPropertyName, StringComparison.OrdinalIgnoreCase));
              itemSchema.Remove(key);
            }
            // Build the first part of the INSERT, including delimited column names:
            if (ReferenceEquals(item, first)) {
              var sbFieldNames = new StringBuilder();
              foreach (var field in itemSchema) {
                string mappedColumnName = this.tableMapping.ColumnMappings.FindByProperty(field.Key).DelimitedColumnName;
                sbFieldNames.AppendFormat("{0},", mappedColumnName);
              }
              var keys = sbFieldNames.ToString().Substring(0, sbFieldNames.Length - 1);
              string stub = "INSERT INTO {0} ({1}) VALUES ";
              insertClause = string.Format(stub, this.tableMapping.DelimitedTableName, string.Join(", ", keys));
              sbSql = new StringBuilder(insertClause);
            }
            foreach (var key in itemSchema.Keys) {
              if (paramCounter + itemSchema.Count >= MAGIC_PG_PARAMETER_LIMIT || rowValueCounter >= MAGIC_PG_ROW_VALUE_LIMIT) {
                dbCommand.CommandText = sbSql.ToString().Substring(0, sbSql.Length - 1);
                commands.Add(dbCommand);
                sbSql = new StringBuilder(insertClause);
                paramCounter = 0;
                rowValueCounter = 0;
                dbCommand = conn.CreateCommand();
                dbCommand.Transaction = transaction;
              }
              // Add the Param groups to the end:
              if (key == "search") {
                //TODO: This will need to be handled differently if SQL Server FTS is implemented:
                sbParamGroup.AppendFormat("to_tsvector(@{0}),", paramCounter.ToString());
              } else {
                sbParamGroup.AppendFormat("@{0},", paramCounter.ToString());
              }
              dbCommand.AddParam(itemSchema[key]);
              paramCounter++;
            }
            // Add the row params to the end of the sql:
            sbSql.AppendFormat("({0}),", sbParamGroup.ToString().Substring(0, sbParamGroup.Length - 1));
            rowValueCounter++;
          }
          dbCommand.CommandText = sbSql.ToString().Substring(0, sbSql.Length - 1);
          commands.Add(dbCommand);
          try {
            foreach (var cmd in commands) {
              rowsAffected += cmd.ExecuteNonQuery();
            }
            transaction.Commit();
          }
          catch (Exception) {
            transaction.Rollback();
          }
        }
      }
      return items;
    }

    /// <summary>
    /// Returns a single row from the database
    /// </summary>
    public virtual T Find<T>(object key) where T : new() {
      var result = new T();
      var sql = this.GetSingleSelect(this.tableMapping.DelimitedTableName, this.PrimaryKeyMapping.DelimitedColumnName + "=@0");
      return Query<T>(sql, key).FirstOrDefault();
    }

    /// <summary>
    /// Enumerates the reader yielding the result - thanks to Jeroen Haegebaert
    /// </summary>
    public IEnumerable<dynamic> Query(string sql, params object[] args) {
      using (var conn = Cache.OpenConnection()) {
        var rdr = this.CreateCommand(sql, conn, args).ExecuteReader();
        while (rdr.Read()) {
          var expando = rdr.RecordToExpando();
          yield return expando;
        }
      }
    }

    /// <summary>
    /// Enumerates the reader yielding the result - thanks to Jeroen Haegebaert
    /// </summary>
    public virtual IEnumerable<T> Query<T>(string sql, params object[] args) where T : new() {
      using (var conn = Cache.OpenConnection()) {
        var rdr = this.CreateCommand(sql, conn, args).ExecuteReader();
        while (rdr.Read()) {
          yield return this.MapReaderToObject<T>(rdr);
        }
      }
    }

    public virtual IEnumerable<T> Query<T>(string sql, DbConnection connection, params object[] args) where T : new() {
      using (var rdr = this.CreateCommand(sql, connection, args).ExecuteReader()) {
        while (rdr.Read()) {
          yield return this.MapReaderToObject<T>(rdr);
        }
      }
    }

    protected virtual T MapReaderToObject<T>(IDataReader reader) where T : new() {
      var item = new T();
      var props = item.GetType().GetProperties();
      foreach (var property in props) {
        if (this.tableMapping.ColumnMappings.ContainsPropertyName(property.Name)) {
          string mappedColumn = this.tableMapping.ColumnMappings.FindByProperty(property.Name).ColumnName;
          int ordinal = reader.GetOrdinal(mappedColumn);
          var val = reader.GetValue(ordinal);
          if (val.GetType() != typeof(DBNull)) {
            property.SetValue(item, reader.GetValue(ordinal));
          }
        }
      }
      return item;
    }

    public virtual DbCommand CreateInsertCommand(T insertItem) {
      DbCommand result = null;
      var expando = insertItem.ToExpando();
      var settings = (IDictionary<string, object>)expando;
      var sbKeys = new StringBuilder();
      var sbVals = new StringBuilder();
      var stub = "INSERT INTO {0} ({1}) \r\n VALUES ({2})";
      result = this.CreateCommand(stub, null);
      int counter = 0;
      if (this.PrimaryKeyMapping.IsAutoIncementing) {
        string mappedPropertyName = this.PrimaryKeyMapping.PropertyName;
        var col = settings.FirstOrDefault(x => x.Key.Equals(mappedPropertyName, StringComparison.OrdinalIgnoreCase));
        settings.Remove(col);
      }
      foreach (var item in settings) {
        sbKeys.AppendFormat("{0},", this.tableMapping.ColumnMappings.FindByProperty(item.Key).DelimitedColumnName);

        //this is a special case for a search directive
        //TODO: This will need to be handled differently if SQL Server FTS is implemented:
        if (item.Value.ToString().StartsWith("to_tsvector")) {
          sbVals.AppendFormat("{0},", item.Value);
        } else {
          sbVals.AppendFormat("@{0},", counter.ToString());
          result.AddParam(item.Value);
        }
        counter++;
      }
      if (counter > 0) {
        var keys = sbKeys.ToString().Substring(0, sbKeys.Length - 1);
        var vals = sbVals.ToString().Substring(0, sbVals.Length - 1);
        var sql = string.Format(stub, this.tableMapping.DelimitedTableName, keys, vals);
        result.CommandText = sql;
      }
      else throw new InvalidOperationException("Can't parse this object to the database - there are no properties set");
      return result;
    }

    /// <summary>
    /// Creates a command for use with transactions - internal stuff mostly, but here for you to play with
    /// </summary>
    public virtual DbCommand CreateUpdateCommand(T updateItem) {
      var expando = updateItem.ToExpando();
      var key = GetPrimaryKey(updateItem);
      var settings = (IDictionary<string, object>)expando;
      var sbKeys = new StringBuilder();
      var stub = "UPDATE {0} SET {1} WHERE {2} = @{3}";
      var args = new List<object>();
      var result = this.CreateCommand(stub, null);
      int counter = 0;
      var mappedPkPropertyName = this.PrimaryKeyMapping.PropertyName;
      foreach (var item in settings) {
        var val = item.Value;
        // Find the property name mapped to this column name:
        if (!item.Key.Equals(mappedPkPropertyName, StringComparison.OrdinalIgnoreCase) && item.Value != null) {
          result.AddParam(val);
          //// use the mapped, delimited database column name:
          string dbColumnName = this.tableMapping.ColumnMappings.FindByProperty(item.Key).DelimitedColumnName;
          sbKeys.AppendFormat("{0} = @{1}, \r\n", dbColumnName, counter.ToString());
          counter++;
        }
      }
      if (counter > 0) {
        //add the key
        result.AddParam(key);
        //strip the last commas
        var keys = sbKeys.ToString().Substring(0, sbKeys.Length - 4);
        result.CommandText = string.Format(stub, this.tableMapping.DelimitedTableName, keys, this.PrimaryKeyMapping.DelimitedColumnName, counter);
      } else {
        throw new InvalidOperationException("No parsable object was sent in - could not divine any name/value pairs");
      }
      return result;
    }

    /// <summary>
    /// Removes one or more records from the DB according to the passed-in WHERE
    /// </summary>
    public virtual DbCommand CreateDeleteCommand(string where = "", object key = null, params object[] args) {
      var sql = string.Format("DELETE FROM {0} ", this.tableMapping.DelimitedTableName);
      if (key != null) {
        sql += string.Format("WHERE {0}=@0", this.PrimaryKeyMapping.DelimitedColumnName);
        args = new object[] { key };
      }
      else if (!string.IsNullOrEmpty(where)) {
        sql += where.Trim().StartsWith("where", StringComparison.OrdinalIgnoreCase) ? where : "WHERE " + where;
      }
      return this.CreateCommand(sql, null, args);
    }

    public virtual object GetPrimaryKey(object o) {
      object result = null;
      var lookup = o.ToDictionary();
      string propName = this.PrimaryKeyMapping.PropertyName;
      var found = lookup.FirstOrDefault(x => x.Key.Equals(propName, StringComparison.OrdinalIgnoreCase));
      result = found.Value;
      return result;
    }

    public virtual void SetPrimaryKey(T item, object value) {
      if (this.PrimaryKeyMapping != null) {
        var props = item.GetType().GetProperties();
        if (item is ExpandoObject) {
          var d = item as IDictionary<string, object>;
          d[this.PrimaryKeyMapping.PropertyName] = value;
        } else {
          // Find the property the PK maps to:
          string mappedPropertyName = this.PrimaryKeyMapping.PropertyName;
          var pkProp = props.FirstOrDefault(x => x.Name.Equals(mappedPropertyName, StringComparison.OrdinalIgnoreCase));
          var converted = Convert.ChangeType(value, pkProp.PropertyType);
          pkProp.SetValue(item, converted);
        }
      }
    }

    /// <summary>
    /// Builds a set of Insert and Update commands based on the passed-on objects.
    /// These objects can be POCOs, Anonymous, NameValueCollections, or Expandos. Objects
    /// With a PK property (whatever PrimaryKeyField is set to) will be created at UPDATEs
    /// </summary>
    public List<DbCommand> BuildCommands(params T[] things) {
      var commands = new List<DbCommand>();
      foreach (var item in things) {
        if (this.HasPrimaryKey(item)) {
          commands.Add(CreateUpdateCommand(item));
        } else {
          commands.Add(CreateInsertCommand(item.ToExpando()));
        }
      }
      return commands;
    }

    //Hooks
    public virtual void Inserted(T item) { }
    public virtual void Updated(T item) { }
    public virtual void Deleted(T item) { }
    public virtual bool BeforeDelete(T item) { return true; }
    public virtual bool BeforeSave(T item) { return true; }


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


    /// <summary>
    /// Checks for the presence of a PK field and a non-default value. 
    /// This method is generally called by the Save method to determine 
    /// whether the object represents a new record, or an existing record. 
    /// </summary>
    public bool HasPrimaryKey(object o) {
      bool keyIsPresent = false;
      var dict = o.ToDictionary();
      Type propertyType = null;
      object propertyValue = null;
      
      object defaultValue;
      if(dict.ContainsKey(this.PrimaryKeyMapping.PropertyName))
      {
        keyIsPresent = true;
        propertyValue = dict[this.PrimaryKeyMapping.PropertyName];
        propertyType = dict[this.PrimaryKeyMapping.PropertyName].GetType();
      }

      if(propertyType.IsValueType) {
        defaultValue = Activator.CreateInstance(propertyType);
      } else {
        defaultValue = null;
      }
      bool hasPK = keyIsPresent && !propertyValue.Equals(defaultValue);
      return hasPK;
    }


    public int Count() {
      return Count(this.tableMapping.DelimitedTableName);
    }

    public int Count(string delimitedTableName, string where = "", params object[] args) {
      return (int)this.Scalar("SELECT COUNT(1) FROM " + delimitedTableName + " " + where, args);
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
    /// This will return an Expando as a Dictionary
    /// </summary>
    IDictionary<string, object> ItemAsDictionary(ExpandoObject item) {
      return (IDictionary<string, object>)item;
    }

    //Checks to see if a key is present based on the passed-in value
    bool ItemContainsKey(string key, ExpandoObject item) {
      var dc = ItemAsDictionary(item);
      return dc.ContainsKey(key);
    }

    /// <summary>
    /// Executes a set of objects as Insert or Update commands based on their property settings, within a transaction.
    /// These objects can be POCOs, Anonymous, NameValueCollections, or Expandos. Objects
    /// With a PK property (whatever PrimaryKeyField is set to) will be created at UPDATEs
    /// </summary>
    public virtual int Save(params T[] things) {
      var commands = this.BuildCommands(things);
      return this.Execute(commands);
    }


    // IMPLEMENTATION FOR IBIGGYSTORE<T>:

    List<T> IBiggyStore<T>.Load() {
      return this.All<T>().ToList();
    }

    void IBiggyStore<T>.SaveAll(List<T> items) {
      throw new NotImplementedException();
    }

    void IBiggyStore<T>.Clear() {
      this.DeleteAll();
    }

    T IBiggyStore<T>.Add(T item) {
      return this.Insert(item);
    }

    List<T> IBiggyStore<T>.Add(List<T> items) {
      this.BulkInsert(items);
      return items;
    }


    // IMPLEMENTATION FOR IUPDATEABLEBIGGYSTORE<T>:

    T IBiggyStore<T>.Update(T item) {
      this.Update(item);
      return item;
    }

    T IBiggyStore<T>.Remove(T item) {
      this.Delete(item);
      return item;
    }

    List<T> IBiggyStore<T>.Remove(List<T> items) {
      this.Delete(items.ToList());
      return items;
    }
  }
}
