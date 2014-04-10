using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using Biggy.Extensions;


namespace Biggy.Postgres {
  public class PGDocumentStore<T> : BiggyDocumentStore<T> where T : new() {
    public PGDocumentStore(DbCache context) : base(context) { }
    public PGDocumentStore(DbCache context, string tableName) : base(context, tableName) { }
    public PGDocumentStore(string connectionStringName) : base(new PGCache(connectionStringName)) { }
    public PGDocumentStore(string connectionStringName, string tableName) : base(new PGCache(connectionStringName), tableName) { }

    public override BiggyRelationalStore<dynamic> getModel() {
      return new PGStore<dynamic>(this.DbCache);
    }

    protected override List<T> TryLoadData() {
      try {
        return this.LoadAll();
      }
      catch (Npgsql.NpgsqlException x) {
        if (x.Message.Contains("does not exist")) {

          //create the table
          var idType = Model.PrimaryKeyMapping.DataType == typeof(int) ? " serial" : "varchar(255)";
          string fullTextColumn = "";
          if (this.FullTextFields.Length > 0) {
            fullTextColumn = ", search tsvector";
          }
          var sql = string.Format("CREATE TABLE {0} ({1} {2} primary key not null, body json not null {3});", this.TableMapping.DelimitedTableName, this.PrimaryKeyMapping.DelimitedColumnName, idType, fullTextColumn);
          this.Model.Execute(sql);
          return TryLoadData();
        } else {
          throw;
        }
      }
    }

    public override T Insert(T item) {
      this.addItem(item);
      if (this.PrimaryKeyMapping.IsAutoIncementing) {
        //// Sync the JSON ID with the serial PK:
        this.Update(item);
      }
      return item;
    }

    internal void addItem(T item) {
      var expando = base.SetDataForDocument(item);
      var dc = expando as IDictionary<string, object>;
      var vals = new List<string>();
      var args = new List<object>();
      var index = 0;

      var keyColumn = dc.FirstOrDefault(x => x.Key.Equals(this.PrimaryKeyMapping.PropertyName, StringComparison.OrdinalIgnoreCase));
      if (this.Model.PrimaryKeyMapping.IsAutoIncementing) {
        //don't update the Primary Key
        dc.Remove(keyColumn);
      }
      foreach (var key in dc.Keys) {
        if (key == "search") {
          vals.Add(string.Format("to_tsvector(@{0})", index));
        } else {
          vals.Add(string.Format("@{0}", index));
        }
        args.Add(dc[key]);
        index++;
      }
      var sb = new StringBuilder();
      sb.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2}) RETURNING {3} as newID;", this.TableMapping.DelimitedTableName, string.Join(",", dc.Keys), string.Join(",", vals), Model.PrimaryKeyMapping.DelimitedColumnName);
      var sql = sb.ToString();
      var newKey = this.Model.Scalar(sql, args.ToArray());
      //set the key
      this.Model.SetPrimaryKey(item, newKey);
    }

    /// <summary>
    /// A high-performance bulk-insert that can drop 10,000 documents in about 900 ms
    /// </summary>
    public override List<T> BulkInsert(List<T> items) {
      const int MAGIC_PG_PARAMETER_LIMIT = 2100;
      const int MAGIC_PG_ROW_VALUE_LIMIT = 1000;
      int rowsAffected = 0;

      var first = items.First();
      string insertClause = "";
      var sbSql = new StringBuilder("");

      using (var connection = this.DbCache.OpenConnection()) {
        using (var tdbTransaction = connection.BeginTransaction(System.Data.IsolationLevel.RepeatableRead)) {
          var commands = new List<DbCommand>();
          // Lock the table, so nothing will disrupt the pk sequence:
          string lockTableSQL = string.Format("LOCK TABLE {0} in ACCESS EXCLUSIVE MODE", this.TableMapping.DelimitedTableName);
          DbCommand dbCommand = this.Model.CreateCommand(lockTableSQL, connection);
          dbCommand.Transaction = tdbTransaction;
          dbCommand.ExecuteNonQuery();

          int nextSerialPk = 0;
          if (Model.PrimaryKeyMapping.IsAutoIncementing) {
            // Now get the next serial Id. ** Need to do this within the transaction/table lock scope **:
            string sequence = string.Format("\"{0}_{1}_seq\"", this.TableMapping.DBTableName, this.PrimaryKeyMapping.ColumnName);
            var sql_get_seq = string.Format("SELECT last_value FROM {0}", sequence);
            dbCommand.CommandText = sql_get_seq;
            // if this is a fresh sequence, the "seed" value is returned. We will assume 1:
            nextSerialPk = Convert.ToInt32(dbCommand.ExecuteScalar());
            // If this is not a fresh sequence, increment:
            if (nextSerialPk > 1) {
              nextSerialPk++;
            }
          }

          var paramCounter = 0;
          var rowValueCounter = 0;
          foreach (var item in items) {
            // Set the soon-to-be inserted serial int value:
            if (Model.PrimaryKeyMapping.IsAutoIncementing) {
              var props = item.GetType().GetProperties();
              var pk = props.First(p => p.Name == this.PrimaryKeyMapping.PropertyName);
              pk.SetValue(item, nextSerialPk);
              nextSerialPk++;
            }
            // Set the JSON object, including the interpolated serial PK
            var itemEx = SetDataForDocument(item);
            var itemSchema = itemEx as IDictionary<string, object>;
            var sbParamGroup = new StringBuilder();
            if (itemSchema.ContainsKey(this.PrimaryKeyMapping.PropertyName) && this.PrimaryKeyMapping.IsAutoIncementing) {
              itemSchema.Remove(this.PrimaryKeyMapping.PropertyName);
            }
            if (ReferenceEquals(item, first)) {
              var sbFieldNames = new StringBuilder();
              foreach (var field in itemSchema) {
                string mappedColumnName = this.TableMapping.ColumnMappings.FindByProperty(field.Key).DelimitedColumnName;
                sbFieldNames.AppendFormat("{0},", mappedColumnName);
              }
              var delimitedColumnNames = sbFieldNames.ToString().Substring(0, sbFieldNames.Length - 1);
              string stub = "INSERT INTO {0} ({1}) VALUES ";
              insertClause = string.Format(stub, this.TableMapping.DelimitedTableName, string.Join(", ", delimitedColumnNames));
              sbSql = new StringBuilder(insertClause);
            }
            foreach (var key in itemSchema.Keys) {
              if (paramCounter + itemSchema.Count >= MAGIC_PG_PARAMETER_LIMIT || rowValueCounter >= MAGIC_PG_ROW_VALUE_LIMIT) {
                dbCommand.CommandText = sbSql.ToString().Substring(0, sbSql.Length - 1);
                commands.Add(dbCommand);
                sbSql = new StringBuilder(insertClause);
                paramCounter = 0;
                rowValueCounter = 0;
                dbCommand = this.Model.CreateCommand("", connection);
                dbCommand.Transaction = tdbTransaction;
              }
              if (key == "search") {
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
            tdbTransaction.Commit();
          }
          catch (Exception) {
            tdbTransaction.Rollback();
          }
        }
      }
      return items;
    }

    /// <summary>
    /// Updates a single T item
    /// </summary>
    public override T Update(T item) {
      var expando = SetDataForDocument(item);
      this.Model.Update(expando);
      return item;
    }

    public override T Delete(T item) {
      Model.Delete(item);
      return item;
    }

    public override List<T> Delete(List<T> items) {
      Model.Delete(items);
      return items;
    }
  }
}
