using System;
using System.Collections.Generic;
using System.Linq;
using Biggy.Mongo.Tests.Support;
using Xunit;

namespace Biggy.Mongo.Tests {
  [Trait("Biggy List with MongoDb", "")]
  public class MongoBackedList_Tests {
    public MongoBackedList_Tests() {
      _dbVerifier.Clear();
    }

    [Fact(DisplayName = "Mongo: loads empty list into memory")]
    public void LoadsEmpty() {
      var widgets = CreateBiggyList();
      Assert.Empty(widgets);
    }

    [Fact(DisplayName = "Mongo: loads a single document into memory")]
    public void LoadSingleDocument() {
      var widget = new Widget() {
        Description = "A widget",
        Expiration = DateTime.Now.AddYears(1),
        Name = "Widget",
        Price = 9.99m,
        Size = 2
      };
      _dbVerifier.Insert(widget);

      var widgets = CreateBiggyList();
      Assert.Equal(1, widgets.Count());

      var found = widgets.First();
      Assert.Equal(widget.Description, found.Description);
      Assert.Equal(widget.Expiration.Year, found.Expiration.Year);
      Assert.Equal(widget.Name, found.Name);
      Assert.Equal(widget.Price, found.Price);
      Assert.Equal(widget.Size, found.Size);
    }

    [Fact(DisplayName = "Mongo: writes 12 metric crap-loads of records into memory and db")]
    public void WriteALot() {
      var data = new List<Widget>();
      for (var i = 0; i < 10000; i++) {
        data.Add(new Widget {
          Description = "A widget",
          Expiration = DateTime.Now.AddYears(1),
          Name = "Widget",
          Price = 9.99m,
          Size = i
        });
      }

      var widgets = CreateBiggyList();
      widgets.Add(data);

      Assert.Equal(data.Count, widgets.Count());
      Assert.Equal(data.Count, _dbVerifier.Count());
    }

    [Fact(DisplayName = "Mongo: queries a range of records from memory")]
    public void Query() {
      var data = new List<Widget>();
      for (var i = 0; i < 10; i++) {
        data.Add(new Widget {
          Description = "A widget",
          Expiration = DateTime.Now.AddYears(1),
          Name = "Widget",
          Price = 9.99m,
          Size = i
        });
      }

      var widgets = CreateBiggyList();
      widgets.Add(data);

      var query = from w in widgets
                  where w.Size > 5 && w.Size < 8
                  select w;

      Assert.Equal(2, query.Count());
    }

    [Fact(DisplayName = "Mongo: Update syncs item to mongo")]
    public void Update() {
      var widget = new Widget() {
        Description = "A widget",
        Expiration = DateTime.Now.AddYears(1),
        Name = "Widget",
        Price = 9.99m,
        Size = 2
      };

      var widgets = CreateBiggyList();
      widgets.Add(widget);

      widget.Name = "I updated this!!";
      widgets.Update(widget);

      var updatedWidget = _dbVerifier.Find(widget.Id);
      Assert.Equal(widget.Name, updatedWidget.Name);

    }

    [Fact(DisplayName = "Mongo: deletes a single record in memory and db")]
    public void Delete() {
      var widget = new Widget() {
        Description = "A widget",
        Expiration = DateTime.Now.AddYears(1),
        Name = "Widget",
        Price = 9.99m,
        Size = 2
      };

      var widgets = CreateBiggyList();
      widgets.Add(widget);
      widgets.Remove(widget);

      Assert.Equal(0, _dbVerifier.Count());
    }

    IBiggy<Widget> CreateBiggyList() {
      var store = new MongoStore<Widget>(Host, Database, Collection);
      var list = new QueryableBiggylist<Widget>(store);
      return list;
    }

    private const string Host = "localhost";
    private const string Database = "biggytest";
    private const string Collection = "widgets";
    private readonly MongoHelper<Widget> _dbVerifier =
        new MongoHelper<Widget>(Host, Database, Collection);
  }
}