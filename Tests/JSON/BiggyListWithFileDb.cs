using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Biggy;
using Biggy.JSON;

namespace Tests.JSON {
  [Trait("Biggy List with File Db", "")]
  public class BiggyListWithFileDb {
    IBiggyStore<Widget> _widgets;
    IQueryableBiggyStore<Widget> _queryableWidgets;

    IBiggy<Widget> _biggyWidgetList;

    public BiggyListWithFileDb() {
      // Set up the store for injection:
      _widgets = new JsonStore<Widget>(dbName: "widgets");
      _queryableWidgets = _widgets as IQueryableBiggyStore<Widget>;
      _widgets.Clear();

      _biggyWidgetList = new Biggy.BiggyList<Widget>(_widgets);
    }

    [Fact(DisplayName = "Initializes Biggy List from JASON Store")]
    public void Initializes_List() {
      Assert.True(_biggyWidgetList != null);
    }

    [Fact(DisplayName = "Adds an item to List and Store")]
    public void Adds_Item_To_List_And_Store() {
      _biggyWidgetList.Clear();
      _biggyWidgetList.Add(new Widget { SKU = "001", Name = "Test widget 1", Price = 2.00M });

      // Reload the list:
      _biggyWidgetList = new BiggyList<Widget>(_widgets);
      var addedItem = _biggyWidgetList.FirstOrDefault(w => w.SKU == "001");
      Assert.True(addedItem != null && _biggyWidgetList.Count() == 1);
    }


    [Fact(DisplayName = "Adds an item to List and Existing Store")]
    public void Adds_Item_To_List_And_Existong_Store() {
      _biggyWidgetList.Clear();
      _biggyWidgetList.Add(new Widget { SKU = "001", Name = "Test widget 1", Price = 2.00M });

      // Reload the list:
      _biggyWidgetList = new BiggyList<Widget>(_widgets);
      _biggyWidgetList.Add(new Widget { SKU = "002", Name = "Test widget 2", Price = 4.00M });

      var addedItem = _biggyWidgetList.FirstOrDefault(w => w.SKU == "001");
      Assert.True(addedItem != null && _biggyWidgetList.Count() == 2);
    }


    [Fact(DisplayName = "Updates an item in List and Store")]
    public void Updates_Item_In_List_And_Store() {
      _biggyWidgetList.Clear();
      var updateMe = _biggyWidgetList.Add(new Widget { SKU = "001", Name = "Test widget 1", Price = 2.00M });
      
      // Update and Save:
      updateMe.Name = "UPDATED";
      _biggyWidgetList.Update(updateMe);

      // Reload the list:
      _biggyWidgetList = new BiggyList<Widget>(_widgets);

      // Grab from the list:
      var updatedInList = _biggyWidgetList.FirstOrDefault(w => w.Name == "UPDATED");

      // Make sure updated in both:
      Assert.True(updatedInList.Name == "UPDATED");
    }


    [Fact(DisplayName = "Updates an item in List and Store with Equals Override")]
    public void Updates_Item_In_List_And_Store_With_Equals_Override() {
      var overrideWidgetList = new BiggyList<OverrideWidget>(new JsonStore<OverrideWidget>());
      overrideWidgetList.Clear();
      overrideWidgetList.Add(new OverrideWidget { SKU = "001", Name = "Test widget 1", Price = 2.00M });

      var updatedItem = new OverrideWidget { SKU = "001", Name = "UPDATED", Price = 2.00M };
      overrideWidgetList.Update(updatedItem);

      // Reload the list:
      overrideWidgetList = new BiggyList<OverrideWidget>(new JsonStore<OverrideWidget>());

      // Grab from the list:
      var updatedInList = overrideWidgetList.FirstOrDefault(w => w.Name == "UPDATED");

      // Make sure updated in both:
      Assert.True(updatedInList.Name == "UPDATED");
    }



    [Fact(DisplayName = "Removes an item from the List and Store")]
    public void Removes_Item_From_List_And_Store() {
      _biggyWidgetList.Clear();
      _biggyWidgetList.Add(new Widget { SKU = "001", Name = "Test widget 1", Price = 2.00M });

      // Reload:
      _biggyWidgetList = new BiggyList<Widget>(_widgets);

      var removeMe = _biggyWidgetList.FirstOrDefault();
      _biggyWidgetList.Remove(removeMe);
      _biggyWidgetList = new BiggyList<Widget>(_widgets);
      Assert.True(_biggyWidgetList.Count() == 0);
    }


    [Fact(DisplayName = "Adds batch of items to List")]
    public void Adds_Batch_Of_Items_To_Store() {
      _biggyWidgetList.Clear();

      int INSERT_QTY = 100;
      var batch = new List<Widget>();
      for (int i = 0; i < INSERT_QTY; i++) {
        batch.Add(new Widget { SKU = string.Format("00{0}", i), Name = string.Format("Test widget {0}", i), Price = 2.00M });
      }
      _biggyWidgetList.Add(batch);

      // Reload the List:
      var itemsInStore = new BiggyList<Widget>(_widgets);
      Assert.True(itemsInStore.Count() == INSERT_QTY);
    }

    [Fact(DisplayName = "Removes Range of items from Store")]
    public void Removes_Range_From_Store() {
      _biggyWidgetList.Clear();

      int INSERT_QTY = 100;
      var batch = new List<Widget>();
      for (int i = 0; i < INSERT_QTY; i++) {
        batch.Add(new Widget { SKU = string.Format("00{0}", i), Name = string.Format("Test widget {0}", i), Price = 2.00M + i });
      }
      _biggyWidgetList.Add(batch);
      _biggyWidgetList = new BiggyList<Widget>(_widgets);

      // Grab a range of items to remove:
      var itemsToRemove = _widgets.Load().Where(w => w.Price > 5 && w.Price <= 20);
      int removedQty = itemsToRemove.Count();

      _biggyWidgetList.Remove(itemsToRemove.ToList());

      // Reload again, just to be sure:
      _biggyWidgetList = new BiggyList<Widget>(_widgets);
      Assert.True(removedQty > 0 && removedQty < INSERT_QTY && _biggyWidgetList.Count() == (INSERT_QTY - removedQty));
    }

  
  }
}
