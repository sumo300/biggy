using System;
using System.Linq;
using Biggy.Core;

namespace Biggy.Data.Json
{
    public class AzureTablesDbCore
    {
        public IDataStore<T> CreateStoreFor<T>() where T : new()
        {
            return null;
        }

        //public AzureTablesDbCore(string dbName = "Data")
        //{
        //    this.DatabaseName = dbName;
        //    this.DbDirectory = GetDefaultDirectory();
        //    Directory.CreateDirectory(this.DbDirectory);
        //}

        //public AzureTablesDbCore(string DbDirectory, string dbName)
        //{
        //    this.DatabaseName = dbName;
        //    this.DbDirectory = Path.Combine(DbDirectory, dbName);
        //    Directory.CreateDirectory(this.DbDirectory);
        //}

        //public virtual string AzureConnectionString { get; set; }

        //public virtual string GetDefaultDirectory()
        //{
        //    string defaultDirectory = "";
        //    var currentDir = Directory.GetCurrentDirectory();
        //    if (currentDir.EndsWith("Debug") || currentDir.EndsWith("Release"))
        //    {
        //        var projectRoot = Directory.GetParent(@"..\..\").FullName;
        //        defaultDirectory = Path.Combine(projectRoot, this.DatabaseName);
        //    }
        //    return defaultDirectory;
        //}

        //public int TryDropTable(string tableName)
        //{
        //    if (!tableName.Contains("."))
        //    {
        //        tableName = tableName + ".json";
        //    }
        //    string filePath = Path.Combine(this.DbDirectory, tableName);
        //    if (File.Exists(filePath))
        //    {
        //        File.Delete(filePath);
        //        return 1;
        //    }
        //    return 0;
        //}

        //public bool TableExists(string tableName)
        //{
        //    return File.Exists(Path.Combine(this.DbDirectory, tableName));
        //}
    }
}