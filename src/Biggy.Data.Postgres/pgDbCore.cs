using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Biggy.Core;

namespace Biggy.Data.Postgres {
  public class pgDbCore : DbCore {

    public IDataStore<T> CreateRelationalStoreFor<T>() where T : new() {
      return new pgRelationalStore<T>(this);
    }

    public IDataStore<T> CreateDocumentStoreFor<T>() where T : new() {
      return new pgDocumentStore<T>(this);
    }

    public pgDbCore(string connectionStringName) : base(connectionStringName) { }

    public override string DbDelimiterFormatString {
      get { return "\"{0}\""; }
    }

    protected override void LoadDbColumnsList() {
      this.DbColumnsList = new List<DbColumnMapping>();
      var sql = ""
        + "SELECT c.TABLE_NAME, c.COLUMN_NAME, kcu.CONSTRAINT_NAME, c.DATA_TYPE, c.CHARACTER_MAXIMUM_LENGTH, tc.CONSTRAINT_TYPE, "
          + "CASE tc.CONSTRAINT_TYPE WHEN 'PRIMARY KEY' THEN CAST(1 AS BIt) ELSE CAST(0 AS Bit) END AS IsPrimaryKey, "
          + "(CASE ((SELECT CASE (LENGTH(pg_get_serial_sequence(quote_ident(c.TABLE_NAME), c.COLUMN_NAME)) > 0) WHEN true THEN 1 ELSE 0 END) + "
          + "(SELECT CASE (SELECT pgc.relkind FROM pg_class pgc WHERE pgc.relname = c.TABLE_NAME || '_' || c.COLUMN_NAME || '_' || 'seq') WHEN 'S' THEN 1 ELSE 0 END)) "
          + "WHEN 0 THEN false ELSE true END) AS IsAuto "
          + "FROM information_schema.columns c "
          + "LEFT OUTER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu "
        + "ON c.TABLE_SCHEMA = kcu.CONSTRAINT_SCHEMA AND c.TABLE_NAME = kcu.TABLE_NAME AND c.COLUMN_NAME = kcu.COLUMN_NAME "
        + "LEFT OUTER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc "
        + "ON kcu.CONSTRAINT_SCHEMA = tc.CONSTRAINT_SCHEMA AND kcu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME "
        + "WHERE c.TABLE_SCHEMA = 'public'";

      using (var dr = this.OpenReader(sql)) {
        while (dr.Read()) {
          var clm = dr["COLUMN_NAME"] as string;
          var newColumnMapping = new DbColumnMapping(this.DbDelimiterFormatString) {
            TableName = dr["TABLE_NAME"] as string,
            ColumnName = clm,
            PropertyName = clm,
            IsPrimaryKey = (bool)dr["IsPrimaryKey"],
            IsAutoIncementing = (bool)dr["IsAuto"]
          };
          this.DbColumnsList.Add(newColumnMapping);
        }
      }
    }

    protected override void LoadDbTableNames() {
      this.DbTableNames = new List<string>();
      var sql = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'public'";
      using (var dr = this.OpenReader(sql)) {
        while (dr.Read()) {
          this.DbTableNames.Add(dr.GetString(0));
        }
      }
    }

    public override bool TableExists(string tableName) {
      string sqlFormat = ""
          + "SELECT COUNT(TABLE_NAME) FROM INFORMATION_SCHEMA.TABLES "
          + "WHERE TABLE_SCHEMA = 'public' "
          + "AND  TABLE_NAME = '{0}'";
      string sql = string.Format(sqlFormat, tableName);
      var result = this.ExecuteScalar(sql);
      return (System.Int64)result > 0;
    }


    public override System.Data.IDbConnection CreateConnection(string connectionString) {
      return new Npgsql.NpgsqlConnection(this.ConnectionString);
    }

    public override System.Data.IDbCommand CreateCommand() {
      return new Npgsql.NpgsqlCommand();
    }
  }
}
