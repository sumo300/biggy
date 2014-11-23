using System;
using System.IO;
using System.Linq;
using Biggy.Data.Json;
using NUnit.Framework;

namespace Tests.Json
{
    [TestFixture()]
    [Category("Json DbCore")]
    public class JsonDbCore_Tests
    {
        private JsonDbCore _db;

        [SetUp]
        public void init()
        {
        }

        [Test()]
        public void Initializes_JsonDb_In_Project_Root_In_Defeault_Folder()
        {
            _db = new JsonDbCore();
            string projectRoot = Directory.GetParent(@"..\..\").FullName;
            string expectedDirectory = Path.Combine(projectRoot, "Data");
            Assert.True(_db.DbDirectory == expectedDirectory);
        }

        [Test()]
        public void Initializes_JsonDb_With_Project_Root_With_Folder_Option()
        {
            string alternateDbFolder = "SillyTestDbName";
            _db = new JsonDbCore(alternateDbFolder);
            string projectRoot = Directory.GetParent(@"..\..\").FullName;
            string expectedDirectory = Path.Combine(projectRoot, alternateDbFolder);
            Assert.True(_db.DbDirectory == expectedDirectory);
        }

        [Test()]
        public void Initializes_JsonDb_With_Alternate_Directory()
        {
            string alternateDbFolder = "SillyTestDbName";
            string alternateDbDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            _db = new JsonDbCore(alternateDbDirectory, alternateDbFolder);
            string targetDirectoryRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string expectedDirectory = Path.Combine(targetDirectoryRoot, alternateDbFolder);
            Assert.True(_db.DbDirectory == expectedDirectory);
        }
    }
}