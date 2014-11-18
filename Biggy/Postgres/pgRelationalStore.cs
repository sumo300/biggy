using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Biggy.Extensions;
using System.Dynamic;
using Biggy.Core;
using System.Data;

namespace Biggy.Postgres {
  public class pgRelationalStore<T> : RelationalStoreBase<T> where T : new() {

    public pgRelationalStore(IDbCore dbCore) : base(dbCore) { }

    public override int Add(IEnumerable<T> items) {
      const int MAGIC_PG_PARAMETER_LIMIT = 2100;
      const int MAGIC_PG_ROW_VALUE_LIMIT = 1000;

      if (items.Count() > 0) {
        string insertBase = "INSERT INTO {0} ({1}) VALUES ";
        var model = new T();
        var properties = model.GetType().GetProperties();
        var insertColumns = new List<string>();
        DbColumnMapping autoPkColumn = null;

        // Build the static part of the INSERT, with the column names:
        foreach (var property in properties) {
          var matchingColumn = this.TableMapping.ColumnMappings.FindByProperty(property.Name);
          if (matchingColumn != null) {
            if (matchingColumn.IsAutoIncementing) {
              // We need this later:
              autoPkColumn = matchingColumn;

              // We have to use some tricks to batch insert the proper sequence values:
              string sequenceName = string.Format("{0}_{1}_seq", this.TableMapping.DBTableName, autoPkColumn.ColumnName);
              int itemCount = items.Count();
              string sqlReservedSequenceValues = string.Format("SELECT nextval('\"{0}\"') FROM generate_series( 1, {1} ) n", sequenceName, itemCount);

              // Load up the reserved values into the Id field for each item:
              using (var dr = this.Database.OpenReader(sqlReservedSequenceValues)) {
                int row = 0;
                while (dr.Read()) {
                  var curr = items.ElementAt(row);
                  var props = curr.GetType().GetProperties();
                  var pkProp = props.FirstOrDefault(p => p.Name == autoPkColumn.PropertyName);
                  var converted = Convert.ChangeType(dr[0], pkProp.PropertyType);
                  pkProp.SetValue(curr, converted, null);
                  row++;
                }
              }
            }
            insertColumns.Add(matchingColumn.DelimitedColumnName);
          }
        }

        var sb = new StringBuilder();
        string valueGroupFormat = "({0})";
        var args = new List<object>();
        var paramIndex = 0;
        var rowValueCounter = 0;

        var commands = new List<System.Data.IDbCommand>();
        sb.AppendFormat(insertBase, this.TableMapping.DelimitedTableName, string.Join(", ", insertColumns.ToArray()));
        var valueGroups = new List<string>();

        foreach (var item in items) {
          var itemEx = item.ToExpando();
          var itemSchema = itemEx as IDictionary<string, object>;
          var parameterPlaceholders = new List<string>();

          // pg imposes limits on the number of params and rows in a single statement:
          if (paramIndex + itemSchema.Count >= MAGIC_PG_PARAMETER_LIMIT || rowValueCounter >= MAGIC_PG_ROW_VALUE_LIMIT) {
            // Grab the sql statement from sb and add a command to the list:
            sb.Append(string.Join(",", valueGroups));
            commands.Add(this.Database.BuildCommand(sb.ToString(), args.ToArray()));

            // Start over:
            sb = new StringBuilder();
            sb.AppendFormat(insertBase, this.TableMapping.DelimitedTableName, string.Join(", ", insertColumns.ToArray()));
            paramIndex = 0;
            parameterPlaceholders.Clear();
            valueGroups.Clear();
            args.Clear();
          }
          foreach (var validProp in this.TableMapping.ColumnMappings.ColumnsByPropertyName) {
            var value = itemSchema[validProp.Key];
            args.Add(value);
            parameterPlaceholders.Add("@" + paramIndex++.ToString());
          }
          //foreach (var kvp in itemSchema) {
          //  args.Add(kvp.Value);
          //  parameterPlaceholders.Add("@" + paramIndex++.ToString());
          //}
          string valueGroup = string.Format(valueGroupFormat, string.Join(",", parameterPlaceholders));
          valueGroups.Add(valueGroup);
        }
        sb.Append(string.Join(",", valueGroups));
        commands.Add(this.Database.BuildCommand(sb.ToString(), args.ToArray()));
        return this.Database.Transact(commands.ToArray());
      }
      return 0;

    }

    public override int Add(T item) {
      var properties = item.GetType().GetProperties();
      var itemEx = item.ToExpando();
      var itemSchema = itemEx as IDictionary<string, object>;
      string returnInsertedId = "";
      System.Reflection.PropertyInfo autoPkProperty = null;

      var insertColumns = new List<string>();
      var args = new List<object>();
      var paramIndex = 0;
      var parameterPlaceholders = new List<string>();

      foreach (var property in properties) {
        var matchingColumn = this.TableMapping.ColumnMappings.FindByProperty(property.Name);
        if (matchingColumn != null) {
          if (!matchingColumn.IsAutoIncementing) {
            insertColumns.Add(matchingColumn.DelimitedColumnName);
            args.Add(itemSchema[property.Name]);
            parameterPlaceholders.Add("@" + paramIndex++.ToString());
          } else {
            // We want the new ID back:
            returnInsertedId = string.Format("RETURNING {0} AS newId", matchingColumn.DelimitedColumnName);
            autoPkProperty = property;
          }
        }
      }
      string insertBase = "INSERT INTO {0} ({1}) VALUES ({2}) {3}";
      string sql = string.Format(
        insertBase, 
        this.TableMapping.DelimitedTableName, 
        string.Join(", ", insertColumns.ToArray()), 
        string.Join(", ", parameterPlaceholders.ToArray()),
        returnInsertedId
      );
      if (autoPkProperty != null) {
        var result = this.Database.ExecuteScalar(sql, args.ToArray());
        autoPkProperty.SetValue(item, result, null);
        return 1;
      } else {
        return this.Database.Transact(this.Database.BuildCommand(sql, args.ToArray()));
      }
    }

    public override int Delete(IEnumerable<T> items) {
      if (items.Count() > 0) {
        string keyColumnNames;
        var keyList = new List<string>();

        // The first pk in the list is what we want, to build a standard IN statement 
        // like this: DELETE FROM myTable WHERE pk1 IN (value1, value2, value3, value4, ...)
        keyColumnNames = this.TableMapping.PrimaryKeyMapping[0].DelimitedColumnName;
        foreach (var item in items) {
          var expando = item.ToExpando();
          var dict = (IDictionary<string, object>)expando;
          var pk = this.TableMapping.PrimaryKeyMapping[0];
          if (pk.DataType == typeof(string)) {
            // Wrap in single quotes
            keyList.Add(string.Format("'{0}'", dict[pk.PropertyName].ToString()));
          } else {
            // Don't wrap:
            keyList.Add(dict[pk.PropertyName].ToString());
          }
        }
        string sqlFormat = "DELETE FROM {0} Where {1} ";
        var keySet = String.Join(",", keyList.ToArray());
        var inStatement = keyColumnNames + "IN (" + keySet + ")";
        string sql = string.Format(sqlFormat, this.TableMapping.DelimitedTableName, inStatement);
        return this.Database.Transact(sql);
      }
      return 0;
    }

    public override int Delete(T item) {
      // Calling the override for update is functionally the same
      // as how we would code for a single update:
      return this.Delete(new T[] { item });
    }

    public override int DeleteAll() {
      string sql = string.Format("DELETE FROM {0}", this.TableMapping.DelimitedTableName);
      return this.Database.Transact(sql);
    }

    public override List<T> TryLoadData() {
      string sql = string.Format("SELECT * FROM {0}", this.TableMapping.DelimitedTableName);
      var result = new List<T>();
      using (var dr = this.Database.OpenReader(sql)) {
        while (dr.Read()) {
          var newItem = this.MapReaderToObject<T>(dr);
          result.Add(newItem);
        }
      }
      return result;
    }

    public override int Update(IEnumerable<T> items) {
      var args = new List<object>();
      var paramIndex = 0;
      string ParameterAssignmentFormat = "{0} = @{1}";
      string sqlFormat = ""
      + "UPDATE {0} SET {1} WHERE {2};";
      var sb = new StringBuilder();

      foreach (var item in items) {
        var ex = item.ToExpando ();
        var dc = ex as IDictionary<string, object>;
        var setValueStatements = new List<string>();
        var pkColumnMapping = this.TableMapping.PrimaryKeyMapping[0];
        foreach (var kvp in dc) {
          if (kvp.Key != pkColumnMapping.PropertyName) {
            args.Add(kvp.Value);
            string delimitedColumnName = this.TableMapping.ColumnMappings.FindByProperty(kvp.Key).DelimitedColumnName;
            string setItem = string.Format(ParameterAssignmentFormat, delimitedColumnName, paramIndex++.ToString());
            setValueStatements.Add(setItem);
          }
        }
        var pkValue = dc[pkColumnMapping.PropertyName];
        args.Add(pkValue);
        string whereCriteria = string.Format(ParameterAssignmentFormat, pkColumnMapping.DelimitedColumnName, paramIndex++.ToString());
        sb.AppendFormat(sqlFormat, this.TableMapping.DelimitedTableName, string.Join(",", setValueStatements), whereCriteria);
      }
      var batchedSQL = sb.ToString();
      return this.Database.Transact(batchedSQL, args.ToArray());
    }

    public override int Update(T item) {
      // Calling the override for update is functionally the same
      // as how we would code for a single update:
      return this.Update(new T[] { item });
    }

  }
}
