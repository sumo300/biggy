using System;
using System.IO;
using System.Linq;
using Biggy.Core;

namespace Biggy.Json
{
    public class JsonDbCore
    {
        public virtual IDataStore<T> CreateStoreFor<T>() where T : new()
        {
            return new JsonStore<T>(this);
        }

        public JsonDbCore(string dbName = "Data")
        {
            this.DatabaseName = dbName;
            this.DbDirectory = GetDefaultDirectory();
            Directory.CreateDirectory(this.DbDirectory);
        }

        public JsonDbCore(string DbDirectory, string dbName)
        {
            this.DatabaseName = dbName;
            this.DbDirectory = Path.Combine(DbDirectory, dbName);
            Directory.CreateDirectory(this.DbDirectory);
        }

        public virtual string DbDirectory { get; set; }

        public virtual string DatabaseName { get; set; }

        public virtual string GetDefaultDirectory()
        {
            string defaultDirectory = "";
            var currentDir = Directory.GetCurrentDirectory();
            if (currentDir.EndsWith("Debug") || currentDir.EndsWith("Release"))
            {
                var projectRoot = Directory.GetParent(@"..\..\").FullName;
                defaultDirectory = Path.Combine(projectRoot, this.DatabaseName);
            }
            return defaultDirectory;
        }

        public virtual int TryDropTable(string tableName)
        {
            if (!tableName.Contains("."))
            {
                tableName = tableName + ".json";
            }
            string filePath = Path.Combine(this.DbDirectory, tableName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return 1;
            }
            return 0;
        }

        public virtual bool TableExists(string tableName)
        {
            return File.Exists(Path.Combine(this.DbDirectory, tableName));
        }
    }
}