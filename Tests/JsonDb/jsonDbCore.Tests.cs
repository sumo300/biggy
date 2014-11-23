using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using Biggy.Json;
using Biggy.Core;
using System.IO;

namespace Tests.Json {
  [TestFixture()]
  [Category("Json DbCore")]
  public class jsonDbCore_Tests {

    jsonDbCore _db;
    
    [SetUp]
    public void init() {

    }

    [Test()]
    public void Initializes_JsonDb_In_Project_Root_In_Defeault_Folder() {
      _db = new jsonDbCore();
      string projectRoot = Directory.GetParent(@"..\..\").FullName;
      string expectedDirectory = Path.Combine(projectRoot, "Data");
      Assert.True(_db.DbDirectory == expectedDirectory);
    }

    [Test()]
    public void Initializes_JsonDb_With_Project_Root_With_Folder_Option() {
      string alternateDbFolder = "SillyTestDbName";
      _db = new jsonDbCore(alternateDbFolder);
      string projectRoot = Directory.GetParent(@"..\..\").FullName;
      string expectedDirectory = Path.Combine(projectRoot, alternateDbFolder);
      Assert.True(_db.DbDirectory == expectedDirectory);
    }

    [Test()]
    public void Initializes_JsonDb_With_Alternate_Directory() {
      string alternateDbFolder = "SillyTestDbName";
      string alternateDbDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

      _db = new jsonDbCore(alternateDbDirectory, alternateDbFolder);
      string targetDirectoryRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
      string expectedDirectory = Path.Combine(targetDirectoryRoot, alternateDbFolder);
      Assert.True(_db.DbDirectory == expectedDirectory);
    }


  }
}

