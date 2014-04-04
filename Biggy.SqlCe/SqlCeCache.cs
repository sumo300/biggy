using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biggy.SqlCe
{
    public class SqlCeCache : Biggy.SQLServer.SQLServerCache
    {
        public SqlCeCache(string connectionStringName) : base(connectionStringName) { }

        public override DbConnection OpenConnection()
        {
            var connection = new System.Data.SqlServerCe.SqlCeConnection(this.ConnectionString);
            connection.Open();
            return connection;
        }

        protected override void LoadDbColumnsList()
        {
            this.DbColumnsList = new List<DbColumnMapping>();
            var sql = ""
                + "SELECT ISC.TABLE_NAME ,ISC.COLUMN_NAME "
                + "      ,CASE WHEN ISI.PRIMARY_KEY = 1 THEN CAST(1 AS Bit) ELSE CAST(0 AS Bit) END AS IsPrimaryKey"
                + "      ,CASE WHEN ISC.AUTOINC_INCREMENT = 1 THEN CAST(1 AS Bit) ELSE CAST(0 AS Bit) END as IsAuto"
                + " FROM INFORMATION_SCHEMA.COLUMNS ISC "
                + " LEFT OUTER JOIN INFORMATION_SCHEMA.INDEXES ISI "
                + "  ON ISC.TABLE_NAME = ISI.TABLE_NAME AND ISC.COLUMN_NAME = ISI.COLUMN_NAME";
            using (var conn = this.OpenConnection())
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        var clm = dr["COLUMN_NAME"] as string;
                        var newColumnMapping = new DbColumnMapping(this.DbDelimiterFormatString)
                        {
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
        }

        protected override void LoadDbTableNames()
        {
            this.DbTableNames = new List<string>();
            var sql = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES";
            using (var conn = this.OpenConnection())
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        this.DbTableNames.Add(dr.GetString(0));
                    }
                }
            }
        }

        public override bool TableExists(string delimitedTableName)
        {
            string select = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'";
            string sql = string.Format(select, delimitedTableName);
            object result;
            using (var conn = OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                result = cmd.ExecuteScalar();
            }
            return result != null;
        }
    }
}
