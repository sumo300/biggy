using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Biggy;
using Biggy.JSON;

namespace Tests.JSON {
  [Trait("Biggy List In Memory", "")]
  public class BiggyListWithFileDbInMemory {
    IBiggyStore<Widget> _widgetStore;
    IUpdateableBiggyStore<Widget> _updateableWidgets;
    IQueryableBiggyStore<Widget> _queryableWidgets;

    IBiggy<Widget> _biggyWidgetList;
    int INSERT_QTY = 100;

    public BiggyListWithFileDbInMemory() {
      // Set up the store for injection:
      _widgetStore = new JsonStore<Widget>(dbName: "widgets");
      _updateableWidgets = _widgetStore as IUpdateableBiggyStore<Widget>;
      _queryableWidgets = _widgetStore as IQueryableBiggyStore<Widget>;
      _widgetStore.Clear();

      _biggyWidgetList = new Biggy.BiggyList<Widget>(_widgetStore);

      // Start with some data in a json file:
      var batch = new List<Widget>();
      for (int i = 0; i < INSERT_QTY; i++) {
        batch.Add(new Widget { SKU = string.Format("00{0}", i), Name = string.Format("Test widget {0}", i), Price = i });
      }
      _biggyWidgetList.Add(batch);
    }

    [Fact(DisplayName = "Initializes In-Memory Biggy List with data from JASON Store")]
    public void Initializes_List() {
      _biggyWidgetList = new BiggyList<Widget>(_widgetStore, inMemory: true);
      Assert.True(_biggyWidgetList.Count() == INSERT_QTY);
    }

    [Fact(DisplayName = "Clears data from list, but not from backing Store")]
    public void Clears_List_But_Not_Store() {
      _biggyWidgetList = new BiggyList<Widget>(_widgetStore, inMemory: true);
      _biggyWidgetList.Clear();
      var storeWidgets = _widgetStore.Load();
      Assert.True(_biggyWidgetList.Count() == 0 && storeWidgets.Count == INSERT_QTY);
    }


    [Fact(DisplayName = "Adds an item to List but not backing Store")]
    public void Adds_Item_To_List_But_Not_Store() {
      _biggyWidgetList = new BiggyList<Widget>(_widgetStore, inMemory: true);
      _biggyWidgetList.Add(new Widget { SKU = "1000", Name = "Test widget 1", Price = 2.00M });
      var storeWidgets = _widgetStore.Load();

      var addedItem = _biggyWidgetList.FirstOrDefault(w => w.SKU == "001");
      Assert.True(addedItem != null && _biggyWidgetList.Count() == INSERT_QTY + 1 && storeWidgets.Count() == INSERT_QTY);
    }


    [Fact(DisplayName = "Updates an item in List but not Store")]
    public void Updates_Item_In_List_But_Not_Store() {
      _biggyWidgetList = new BiggyList<Widget>(_widgetStore, inMemory: true);
      var updateMe = _biggyWidgetList.FirstOrDefault(w => w.SKU == "001");
      
      // Update and Save:
      updateMe.Name = "UPDATED";
      _biggyWidgetList.Update(updateMe);

      // Grab from the list:
      var updatedInList = _biggyWidgetList.FirstOrDefault(w => w.SKU == "001");
      var storeWidgets = _widgetStore.Load();
      var originalSotreWidget = storeWidgets.FirstOrDefault(w => w.SKU == "001");

      // Make sure updated in List, but not store:
      Assert.True(updatedInList.Name == "UPDATED" && originalSotreWidget.Name != "UPDATED");
    }


    [Fact(DisplayName = "Removes an item from the List but not Store")]
    public void Removes_Item_From_List_But_Not_Store() {
      _biggyWidgetList = new BiggyList<Widget>(_widgetStore, inMemory: true);

      var removeMe = _biggyWidgetList.FirstOrDefault();
      _biggyWidgetList.Remove(removeMe);
      var storeWidgets = _widgetStore.Load();
      Assert.True(_biggyWidgetList.Count() == INSERT_QTY -1 && storeWidgets.Count == INSERT_QTY);
    }


    [Fact(DisplayName = "Adds batch of items to List but not Store")]
    public void Adds_Batch_Of_Items_To_List_But_Not_Store() {
      _biggyWidgetList = new BiggyList<Widget>(_widgetStore, inMemory: true);

      int INSERT_QTY = 100;
      var batch = new List<Widget>();
      for (int i = 0; i < INSERT_QTY; i++) {
        batch.Add(new Widget { SKU = string.Format("100{0}", i), Name = string.Format("Test widget {0}", i), Price = 2.00M });
      }
      _biggyWidgetList.Add(batch);

      // Reload the List:
      var storeWidgets = _widgetStore.Load();
      Assert.True(_biggyWidgetList.Count() == 2 * INSERT_QTY && storeWidgets.Count() == INSERT_QTY);
    }

    [Fact(DisplayName = "Removes Range of items from List but not Store")]
    public void Removes_Range_From_List_But_Not_Store() {
      _biggyWidgetList = new BiggyList<Widget>(_widgetStore, inMemory: true);

      // Grab a range of items to remove:
      var itemsToRemove = _biggyWidgetList.Where(w => w.Price > 5 && w.Price <= 20);
      int removedQty = itemsToRemove.Count();

      _biggyWidgetList.Remove(itemsToRemove.ToList());

      var storeWidgets = _widgetStore.Load();
      Assert.True(_biggyWidgetList.Count() < storeWidgets.Count());
    }


    [Fact(DisplayName = "Initializes In-Memory Biggy List with no store")]
    public void Initializes_List_With_No_Store() {
      _biggyWidgetList = new BiggyList<Widget>(null);
      var batch = new List<Widget>();
      for (int i = 0; i < INSERT_QTY; i++) {
        batch.Add(new Widget { SKU = string.Format("00{0}", i), Name = string.Format("Test widget {0}", i), Price = i });
      }
      _biggyWidgetList.Add(batch);

      Assert.True(_biggyWidgetList.Count() == INSERT_QTY);
    }

  
  }
}
