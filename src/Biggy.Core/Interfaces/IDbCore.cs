using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Biggy.Core
{
    public interface IDbCore
    {
        IDbCommand BuildCommand(string sql, params object[] args);

        string ConnectionString { get; set; }

        List<DbColumnMapping> DbColumnsList { get; set; }

        string DbDelimiterFormatString { get; }

        List<string> DbTableNames { get; set; }

        IEnumerable<T> Execute<T>(string sql, params object[] args) where T : new();

        IEnumerable<dynamic> ExecuteDynamic(string sql, params object[] args);

        object ExecuteScalar(string sql, params object[] args);

        T ExecuteSingle<T>(string sql, params object[] args) where T : new();

        dynamic ExecuteSingleDynamic(string sql, params object[] args);

        DBTableMapping getTableMappingFor<T>() where T : new();

        void LoadSchemaInfo();

        IDataReader OpenReader(string sql, params object[] args);

        bool TableExists(string tableName);

        int Transact(params System.Data.IDbCommand[] cmds);

        int Transact(string sql, params object[] args);

        int TryDropTable(string tableName);

        IDbConnection CreateConnection(string connectionStringName);

        IDbCommand CreateCommand();

        int TransactDDL(string sql, params object[] args);

        int TransactDDL(params System.Data.IDbCommand[] cmds);
    }
}