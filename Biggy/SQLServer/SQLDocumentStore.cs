using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using Biggy.Extensions;

namespace Biggy.SQLServer
{
  public class SQLDocumentStore<T> : BiggyDocumentStore<T> where T : new()
  {
    public SQLDocumentStore(BiggyRelationalContext context) : base(context) { }
    public SQLDocumentStore(BiggyRelationalContext context, string tableName) : base(context, tableName) { }

    public SQLDocumentStore(string connectionStringName) : base(new SQLServerContext(connectionStringName)) { }
    public SQLDocumentStore(string connectionStringName, string tableName) : base(new SQLServerContext(connectionStringName), tableName) { }

    protected override List<T> TryLoadData()
    {
      try
      {
        return this.LoadAll();
      }
      catch (System.Data.SqlClient.SqlException x)
      {
        if (x.Message.Contains("Invalid object name"))
        {

          //create the table
          var idType = Model.PrimaryKeyMapping.DataType == typeof(int) ? " int identity(1,1)" : "nvarchar(255)";
          string fullTextColumn = "";
          if (this.FullTextFields.Length > 0)
          {
            fullTextColumn = ", search nvarchar(MAX)";
          }
          var sql = string.Format("CREATE TABLE {0} ({1} {2} primary key not null, body nvarchar(MAX) not null {3});", this.TableMapping.DelimitedTableName, this.PrimaryKeyMapping.DelimitedColumnName, idType, fullTextColumn);
          this.Context.Execute(sql);
          //if (this.FullTextFields.Length > 0) {
          //  var indexSQL = string.Format("CREATE FULL TEXT INDEX ON {0}({1})",this.TableName,string.Join(",",this.FullTextFields));
          //  this.Model.Execute(indexSQL);
          //}
          return this.TryLoadData();
        }
        else
        {
          throw;
        }
      }
    }

    public override T Insert(T item)
    {
      this.addItem(item);
      if (this.PrimaryKeyMapping.IsAutoIncementing)
      {
        //// Sync the JSON ID with the serial PK:
        //var ex = this.SetDataForDocument(item);
        this.Update(item);
      }
      return item;
    }

    internal void addItem(T item)
    {
      var expando = SetDataForDocument(item);
      var dc = expando as IDictionary<string, object>;
      var vals = new List<string>();
      var args = new List<object>();
      var index = 0;

      var keyColumn = dc.FirstOrDefault(x => x.Key.Equals(this.PrimaryKeyMapping.PropertyName, StringComparison.OrdinalIgnoreCase));
      if (this.PrimaryKeyMapping.IsAutoIncementing)
      {
        //don't update the Primary Key
        dc.Remove(keyColumn);
      }
      foreach (var key in dc.Keys)
      {
        vals.Add(string.Format("@{0}", index));
        args.Add(dc[key]);
        index++;
      }
      var sb = new StringBuilder();
      sb.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2}); SELECT SCOPE_IDENTITY() as newID;", this.TableMapping.DelimitedTableName, string.Join(",", dc.Keys), string.Join(",", vals));
      var sql = sb.ToString();
      var newKey = this.Context.Scalar(sql, args.ToArray());
      //set the key
      this.SetPrimaryKey(item, newKey);
    }

    /// <summary>
    /// A high-performance bulk-insert that can drop 10,000 documents in about 500ms
    /// </summary>
    public override List<T> BulkInsert(List<T> items)
    {
      // These are SQL Server Max values:
      const int MAGIC_SQL_PARAMETER_LIMIT = 2100;
      const int MAGIC_SQL_ROW_VALUE_LIMIT = 1000;
      int rowsAffected = 0;

      var first = items.First();

      string insertClause = "";
      var sbSql = new StringBuilder("");

      using (var connection = this.Context.OpenConnection())
      {
        using (var tdbTransaction = connection.BeginTransaction(System.Data.IsolationLevel.RepeatableRead))
        {
          var commands = new List<DbCommand>();
          // Lock the table, so nothing will disrupt the pk sequence:
          string lockTableSQL = string.Format("SELECT 1 FROM {0} WITH(TABLOCKX) ", this.TableMapping.DelimitedTableName);
          DbCommand dbCommand = this.Context.CreateCommand(lockTableSQL, connection);
          dbCommand.Transaction = tdbTransaction;
          dbCommand.ExecuteNonQuery();

          int nextSerialPk = 0;
          if (this.PrimaryKeyMapping.IsAutoIncementing)
          {
            // Now get the next Identity Id. ** Need to do this within the transaction/table lock scope **:
            // NOTE: The application must have ownership permission on the table to do this!!
            var sql_get_seq = string.Format("SELECT IDENT_CURRENT ('{0}' )", this.TableMapping.DelimitedTableName);
            dbCommand.CommandText = sql_get_seq;
            // if this is a fresh sequence, the "seed" value is returned. We will assume 1:
            nextSerialPk = Convert.ToInt32(dbCommand.ExecuteScalar());
            if (nextSerialPk > 1)
            {
              nextSerialPk++;
            }
          }

          var paramCounter = 0;
          var rowValueCounter = 0;
          foreach (var item in items)
          {
            // Set the soon-to-be inserted serial int value:
            if (this.PrimaryKeyMapping.IsAutoIncementing)
            {
              var props = item.GetType().GetProperties();
              var pk = props.First(p => p.Name == this.PrimaryKeyMapping.PropertyName);
              pk.SetValue(item, nextSerialPk);
              nextSerialPk++;
            }
            // Set the JSON object, including the interpolated serial PK
            var itemEx = SetDataForDocument(item);
            var itemSchema = itemEx as IDictionary<string, object>;
            var sbParamGroup = new StringBuilder();
            if (itemSchema.ContainsKey(this.PrimaryKeyMapping.PropertyName) && this.PrimaryKeyMapping.IsAutoIncementing)
            {
              itemSchema.Remove(this.PrimaryKeyMapping.PropertyName);
            }

            if (ReferenceEquals(item, first))
            {
              var sbFieldNames = new StringBuilder();
              foreach (var field in itemSchema)
              {
                string mappedColumnName = this.TableMapping.ColumnMappings.FindByProperty(field.Key).DelimitedColumnName;
                sbFieldNames.AppendFormat("{0},", mappedColumnName);
              }
              var delimitedColumnNames = sbFieldNames.ToString().Substring(0, sbFieldNames.Length - 1);
              string stub = "INSERT INTO {0} ({1}) VALUES ";
              insertClause = string.Format(stub, this.TableMapping.DelimitedTableName, string.Join(", ", delimitedColumnNames));
              sbSql = new StringBuilder(insertClause);
            }
            foreach (var key in itemSchema.Keys)
            {
              if (paramCounter + itemSchema.Count >= MAGIC_SQL_PARAMETER_LIMIT || rowValueCounter >= MAGIC_SQL_ROW_VALUE_LIMIT)
              {
                dbCommand.CommandText = sbSql.ToString().Substring(0, sbSql.Length - 1);
                commands.Add(dbCommand);
                sbSql = new StringBuilder(insertClause);
                paramCounter = 0;
                rowValueCounter = 0;
                dbCommand = this.Context.CreateCommand("", connection);
                dbCommand.Transaction = tdbTransaction;
              }
              // FT SEARCH STUFF SHOULD GO HERE
              sbParamGroup.AppendFormat("@{0},", paramCounter.ToString());
              dbCommand.AddParam(itemSchema[key]);
              paramCounter++;
            }
            // Add the row params to the end of the sql:
            sbSql.AppendFormat("({0}),", sbParamGroup.ToString().Substring(0, sbParamGroup.Length - 1));
            rowValueCounter++;
          }

          dbCommand.CommandText = sbSql.ToString().Substring(0, sbSql.Length - 1);
          commands.Add(dbCommand);
          try
          {
            foreach (var cmd in commands)
            {
              rowsAffected += cmd.ExecuteNonQuery();
            }
            tdbTransaction.Commit();
          }
          catch (Exception)
          {
            tdbTransaction.Rollback();
          }
        }
      }
      return items;
    }

    /// <summary>
    /// Updates a single T item
    /// </summary>
    public override T Update(T item)
    {
      var expando = SetDataForDocument(item);
      var dc = expando as IDictionary<string, object>;
      //this.Model.Insert(expando);
      var index = 0;
      var sb = new StringBuilder();
      var args = new List<object>();
      sb.AppendFormat("UPDATE {0} SET ", this.TableMapping.DelimitedTableName);
      foreach (var key in dc.Keys)
      {
        var stub = string.Format("{0}=@{1},", key, index);
        args.Add(dc[key]);
        index++;
        if (index == dc.Keys.Count)
          stub = stub.Substring(0, stub.Length - 1);
        sb.Append(stub);
      }
      sb.Append(";");
      var sql = sb.ToString();
      //this.Model.Execute(sql, args.ToArray());
      this.Model.Update(expando);
      return item;
    }
  }
}
