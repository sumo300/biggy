using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Biggy.Core;

// This is unfortunate, but it works:
#if __MonoCS__
  using SQLiteConnectionAlias = Mono.Data.Sqlite.SqliteConnection;
  using SQLiteCommandAlias = Mono.Data.Sqlite.SqliteCommand;
  using SQLiteExceptionAlias = Mono.Data.Sqlite.SqliteException;
#else

using SQLiteConnectionAlias = System.Data.SQLite.SQLiteConnection;
using SQLiteCommandAlias = System.Data.SQLite.SQLiteCommand;
using SQLiteExceptionAlias = System.Data.SQLite.SQLiteException;
using System.Data;

#endif

namespace Biggy.Data.Sqlite
{
    public class SqliteDbCore : DbCore
    {
        public IDataStore<T> CreateRelationalStoreFor<T>() where T : new()
        {
            return new SqliteRelationalStore<T>(this);
        }

        public IDataStore<T> CreateDocumentStoreFor<T>() where T : new()
        {
            return new SqliteDocumentStore<T>(this);
        }

        private string _defaultDbName = "data.db";

        public string DBDirectory { get; set; }

        public string DBName { get; set; }

        public override string ConnectionString
        {
            get
            {
                return string.Format("Data Source = {0}", Path.Combine(DBDirectory, DBName));
            }
            set
            {
                throw new ReadOnlyException("This property is Read Only in this implementation");
            }
        }

        public SqliteDbCore()
        {
            this.DBDirectory = this.GetDefaultDirectory();
            this.DBName = _defaultDbName;
            this.CreateDbIfNotExists();
            this.LoadSchemaInfo();
        }

        public SqliteDbCore(string connectionStringName)
        {
            string connString = "";
            try
            {
                connString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
                string dbFileName = connString.Split('=').Last();
                this.DBName = Path.GetFileName(dbFileName);
                this.DBDirectory = Path.GetDirectoryName(dbFileName);
            }
            catch
            {
                this.DBDirectory = this.GetDefaultDirectory();
                if (connectionStringName.Contains("."))
                {
                    this.DBName = connectionStringName;
                }
                else
                {
                    this.DBName = connectionStringName + ".db";
                }
            }
            this.CreateDbIfNotExists();
            this.LoadSchemaInfo();
        }

        public SqliteDbCore(string dbDirectory, string databaseName)
        {
            this.DBDirectory = dbDirectory;
            this.DBName = databaseName;
            this.CreateDbIfNotExists();
            this.LoadSchemaInfo();
        }

        private string GetDefaultDirectory()
        {
            string defaultDirectory = "";
            var currentDir = Directory.GetCurrentDirectory();
            if (currentDir.EndsWith("Debug") || currentDir.EndsWith("Release"))
            {
                var projectRoot = Directory.GetParent(@"..\..\").FullName;
                defaultDirectory = Path.Combine(projectRoot, "Data");
            }
            return defaultDirectory;
        }

        private void CreateDbIfNotExists()
        {
            string path = Path.Combine(this.DBDirectory, this.DBName);
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(this.DBDirectory);
                SQLiteConnectionAlias.CreateFile(Path.Combine(this.DBDirectory, this.DBName));
            }
        }

        public override bool TableExists(string tableName)
        {
            object result;
            string sql = string.Format("select count(name) from sqlite_master where name = '{0}'", tableName);
            using (var cn = new SQLiteConnectionAlias(this.ConnectionString))
            {
                using (var cmd = new SQLiteCommandAlias(sql, cn))
                {
                    cn.Open();
                    try
                    {
                        result = cmd.ExecuteScalar();
                    }
                    catch (SQLiteExceptionAlias)
                    {
                        throw;
                    }
                    finally
                    {
                        cn.Close();
                    }
                }
            }
            return (System.Int64)result > 0;
        }

        public override string DbDelimiterFormatString
        {
            get { return "\"{0}\""; }
        }

        protected override void LoadDbColumnsList()
        {
            this.DbColumnsList = new List<DbColumnMapping>();
            var sqlPragma = "PRAGMA table_info({0})";
            foreach (var tableName in DbTableNames)
            {
                var schemaSql = string.Format("SELECT 1 FROM sqlite_master WHERE tbl_name='{0}' and sql LIKE '%WITHOUT ROWID%'", tableName);
                var noRowId = Convert.ToInt32(this.ExecuteScalar(schemaSql));

                using (var dr = this.OpenReader(string.Format(sqlPragma, tableName)))
                {
                    while (dr.Read())
                    {
                        var clm = dr[1] as string;
                        string dbType = dr.GetString(2);
                        bool isPk = dr.GetBoolean(5);
                        var newColumnMapping = new DbColumnMapping(this.DbDelimiterFormatString)
                        {
                            TableName = tableName,
                            ColumnName = clm,
                            PropertyName = clm,
                            IsPrimaryKey = dr.GetBoolean(5),
                            IsAutoIncementing = (isPk && (dbType.Equals("integer", StringComparison.InvariantCultureIgnoreCase)) && (noRowId == 0))
                        };
                        this.DbColumnsList.Add(newColumnMapping);
                    }
                }
            }
        }

        protected override void LoadDbTableNames()
        {
            this.DbTableNames = new List<string>();
            var sql = "SELECT name from sqlite_master where type='table';";
            using (var dr = this.OpenReader(sql))
            {
                while (dr.Read())
                {
                    this.DbTableNames.Add(dr.GetString(0));
                }
            }
        }

        public override IDbConnection CreateConnection(string connectionString)
        {
            return new SQLiteConnectionAlias(connectionString);
        }

        public override IDbCommand CreateCommand()
        {
            return new SQLiteCommandAlias();
        }
    }
}