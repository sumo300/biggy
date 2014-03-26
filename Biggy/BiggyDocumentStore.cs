using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biggy.Extensions;
using System.Dynamic;
using Newtonsoft.Json;

namespace Biggy
{
  public abstract class BiggyDocumentStore<T> : IBiggyStore<T>, IUpdateableBiggyStore<T>, IQueryableBiggyStore<T> where T : new()
  {
    public abstract T Insert(T item);
    public abstract List<T> BulkInsert(List<T> items);
    public abstract T Update(T item);
    public abstract T Delete(T item);
    public abstract List<T> Delete(List<T> items);
    protected abstract List<T> TryLoadData();

    public abstract BiggyRelationalStore<dynamic> getModel();

      public string[] FullTextFields { get; set; }
    public BiggyRelationalStore<dynamic> Model { get; set; }
    public DbHost Context { get; set; }

    public DBTableMapping TableMapping  {
      get { return this.Model.tableMapping; }
      set { this.Model.tableMapping = value; }
    }

    public DbColumnMapping PrimaryKeyMapping {
      get { return this.Model.PrimaryKeyMapping; }
      set { this.Model.PrimaryKeyMapping = value; }
    }

    public virtual object GetPrimaryKey(T item) {
      return Model.GetPrimaryKey(item);
    }

    public virtual void SetPrimaryKey(T item, object value) {
      Model.SetPrimaryKey(item, value);
    }

    public BiggyDocumentStore(DbHost context) {
      this.Context = context;
      this.Model = this.getModel();

      //this.Model = new BiggyRelationalStore<dynamic>(context);
      this.TableMapping = this.getTableMappingForT();
      this.PrimaryKeyMapping = this.TableMapping.PrimaryKeyMapping[0];
      SetFullTextColumns();
      TryLoadData();
    }

    string _userDefinedTableName = "";
    public BiggyDocumentStore(DbHost context, string tableName)
    {
      _userDefinedTableName = tableName;
      this.Context = context;

      this.Model = this.getModel();
      //this.Model = new BiggyRelationalStore<dynamic>(context);
      this.TableMapping = this.getTableMappingForT();
      this.PrimaryKeyMapping = this.TableMapping.PrimaryKeyMapping[0];
      SetFullTextColumns();
      TryLoadData();
    }

    public void CreateDocumentTableForT(List<string> columnDefs) {
      string columnDefinitions = string.Join(",", columnDefs.ToArray());
      var sql = string.Format("CREATE TABLE {0} ({1});", this.TableMapping.DelimitedTableName, columnDefinitions);
      this.Context.Execute(sql);
    }

    public DBTableMapping getTableMappingForT() {
      var result = new DBTableMapping(this.Context.DbDelimiterFormatString);
      result.DBTableName = this.DecideTableName();
      var pk = this.getPrimaryKeyForT();
      result.PrimaryKeyMapping.Add(pk);
      result.ColumnMappings.Add(pk);
      result.ColumnMappings.Add("body", "body");
      result.ColumnMappings.Add("search", "search");
      return result;
    }

    internal virtual string GetBaseName() {
      return typeof(T).Name;
    }

    string DecideTableName() {
      if(string.IsNullOrEmpty(_userDefinedTableName)) {
        //use the type name
        var baseName = this.GetBaseName();
        return Inflector.Inflector.Pluralize(baseName);
      }
      return _userDefinedTableName;
    }

    void SetFullTextColumns() {
      var foundProps = new T().LookForCustomAttribute(typeof(FullTextAttribute));
      this.FullTextFields = foundProps.Select(x => x.Name).ToArray();
    }

    DbColumnMapping getPrimaryKeyForT() {
      DbColumnMapping result = new DbColumnMapping(this.Context.DbDelimiterFormatString);
      result.TableName = this.DecideTableName();
      var baseName = this.GetBaseName();
      var acceptableKeys = new string[] { "ID", baseName + "ID" };
      var props = typeof(T).GetProperties();
      var conventionalKey = props.FirstOrDefault(x => x.Name.Equals("id", StringComparison.OrdinalIgnoreCase)) ??
         props.FirstOrDefault(x => x.Name.Equals(baseName + "ID", StringComparison.OrdinalIgnoreCase));

      if (conventionalKey == null) {
        var foundProp = props
          .FirstOrDefault(p => p.GetCustomAttributes(false)
            .Any(a => a.GetType() == typeof(PrimaryKeyAttribute)));

        if (foundProp != null) {
          result.ColumnName = foundProp.Name;
          result.DataType = foundProp.PropertyType;
          result.PropertyName = foundProp.Name;
        }
      } else {
        result.DataType = typeof(int);
        result.ColumnName = conventionalKey.Name;
        result.PropertyName = conventionalKey.Name;
      }
      result.IsPrimaryKey = true;
      result.IsAutoIncementing = result.DataType == typeof(int);
      if (String.IsNullOrWhiteSpace(result.ColumnName)) {
        throw new InvalidOperationException("Can't tell what the primary key is. You can use ID, " + baseName + "ID, or specify with the PrimaryKey attribute");
      }
      return result;
    }

    protected ExpandoObject SetDataForDocument(T item) {
      var json = JsonConvert.SerializeObject(item);
      var key = this.GetPrimaryKey(item);
      var expando = new ExpandoObject();
      var dict = expando as IDictionary<string, object>;

      dict[this.PrimaryKeyMapping.PropertyName] = key;
      dict["body"] = json;

      if (this.FullTextFields.Length > 0) {
        //get the data from the item passed in
        var itemdc = item.ToDictionary();
        var vals = new List<string>();
        foreach (var ft in this.FullTextFields) {
          var val = itemdc[ft] == null ? "" : itemdc[ft].ToString();
          vals.Add(val);
        }
        dict["search"] = string.Join(",", vals);
      }
      return expando;
    }


    public List<T> LoadAll() {
      var list = new List<T>();
      var results = this.Model.Query("select body from " + this.TableMapping.DelimitedTableName);//this.Model.All<T>().ToList();
      //our results are all dynamic - but all we care about is the body
      var sb = new StringBuilder();
      foreach (var item in results) {
        sb.AppendFormat("{0},", item.body);
      }
      // Can't take a substring of a zero-length string:
      if (sb.Length > 0) {
        var scrunched = sb.ToString();
        var stripped = scrunched.Substring(0, scrunched.Length - 1);
        var json = string.Format("[{0}]", stripped);
        list = JsonConvert.DeserializeObject<List<T>>(json);
      }
      return list;
    }

    List<T> IBiggyStore<T>.Load() {
      return this.LoadAll();
    }

    void IBiggyStore<T>.SaveAll(List<T> items) {
      throw new NotImplementedException();
    }

    void IBiggyStore<T>.Clear() {
      var store = this.Model as IBiggyStore<dynamic>;
      store.Clear();
    }

    T IBiggyStore<T>.Add(T item) {
      return this.Insert(item);
    }

    List<T> IBiggyStore<T>.Add(List<T> items) {
      return this.BulkInsert(items.ToList());
    }

    T IUpdateableBiggyStore<T>.Update(T item) {
      return this.Update(item);
    }

    T IUpdateableBiggyStore<T>.Remove(T item) {
      return this.Delete(item);
    }

    List<T> IUpdateableBiggyStore<T>.Remove(List<T> items) {
      return this.Delete(items.ToList());
    }

    IQueryable<T> IQueryableBiggyStore<T>.AsQueryable()
    {
      return this.LoadAll().AsQueryable();
    }
  }
}
