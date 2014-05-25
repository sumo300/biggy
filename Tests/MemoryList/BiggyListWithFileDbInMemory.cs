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
    IBiggy<Widget> _biggyMemoryList;
    int INSERT_QTY = 100;

    public BiggyListWithFileDbInMemory() {
      // Set up the store for injection:
      _widgetStore = new JsonStore<Widget>(dbName: "widgets");
      _widgetStore.Clear();

      _biggyMemoryList = new Biggy.BiggyList<Widget>(_widgetStore);

      // Start with some data in a json file:
      var batch = new List<Widget>();
      for (int i = 0; i < INSERT_QTY; i++) {
        batch.Add(new Widget { SKU = string.Format("00{0}", i), Name = string.Format("Test widget {0}", i), Price = i });
      }
      _biggyMemoryList.Add(batch);
    }

    [Fact(DisplayName = "Initializes In-Memory Biggy List with data from JASON Store")]
    public void Initializes_List() {
      _biggyMemoryList = new BiggyList<Widget>(_widgetStore, inMemory: true);
      Assert.True(_biggyMemoryList.Count() == INSERT_QTY);
    }

    [Fact(DisplayName = "Clears data from list, but not from backing Store")]
    public void Clears_List_But_Not_Store() {
      _biggyMemoryList = new BiggyList<Widget>(_widgetStore, inMemory: true);
      _biggyMemoryList.Clear();
      var storeWidgets = _widgetStore.Load();
      Assert.True(_biggyMemoryList.Count() == 0 && storeWidgets.Count == INSERT_QTY);
    }


    [Fact(DisplayName = "Adds an item to List but not backing Store")]
    public void Adds_Item_To_List_But_Not_Store() {
      _biggyMemoryList = new BiggyList<Widget>(_widgetStore, inMemory: true);
      _biggyMemoryList.Add(new Widget { SKU = "1000", Name = "Test widget 1", Price = 2.00M });
      var storeWidgets = _widgetStore.Load();

      var addedItem = _biggyMemoryList.FirstOrDefault(w => w.SKU == "001");
      Assert.True(addedItem != null && _biggyMemoryList.Count() == INSERT_QTY + 1 && storeWidgets.Count() == INSERT_QTY);
    }


    [Fact(DisplayName = "Updates an item in List but not Store")]
    public void Updates_Item_In_List_But_Not_Store() {
      _biggyMemoryList = new BiggyList<Widget>(_widgetStore, inMemory: true);
      var updateMe = _biggyMemoryList.FirstOrDefault(w => w.SKU == "001");
      
      // Update and Save:
      updateMe.Name = "UPDATED";
      _biggyMemoryList.Update(updateMe);

      // Grab from the list:
      var updatedInList = _biggyMemoryList.FirstOrDefault(w => w.SKU == "001");
      var storeWidgets = _widgetStore.Load();
      var originalSotreWidget = storeWidgets.FirstOrDefault(w => w.SKU == "001");

      // Make sure updated in List, but not store:
      Assert.True(updatedInList.Name == "UPDATED" && originalSotreWidget.Name != "UPDATED");
    }


    [Fact(DisplayName = "Removes an item from the List but not Store")]
    public void Removes_Item_From_List_But_Not_Store() {
      _biggyMemoryList = new BiggyList<Widget>(_widgetStore, inMemory: true);

      var removeMe = _biggyMemoryList.FirstOrDefault();
      _biggyMemoryList.Remove(removeMe);
      var storeWidgets = _widgetStore.Load();
      Assert.True(_biggyMemoryList.Count() == INSERT_QTY -1 && storeWidgets.Count == INSERT_QTY);
    }


    [Fact(DisplayName = "Adds batch of items to List but not Store")]
    public void Adds_Batch_Of_Items_To_List_But_Not_Store() {
      _biggyMemoryList = new BiggyList<Widget>(_widgetStore, inMemory: true);

      int INSERT_QTY = 100;
      var batch = new List<Widget>();
      for (int i = 0; i < INSERT_QTY; i++) {
        batch.Add(new Widget { SKU = string.Format("100{0}", i), Name = string.Format("Test widget {0}", i), Price = 2.00M });
      }
      _biggyMemoryList.Add(batch);

      // Reload the List:
      var storeWidgets = _widgetStore.Load();
      Assert.True(_biggyMemoryList.Count() == 2 * INSERT_QTY && storeWidgets.Count() == INSERT_QTY);
    }

    [Fact(DisplayName = "Removes Range of items from List but not Store")]
    public void Removes_Range_From_List_But_Not_Store() {
      _biggyMemoryList = new BiggyList<Widget>(_widgetStore, inMemory: true);

      // Grab a range of items to remove:
      var itemsToRemove = _biggyMemoryList.Where(w => w.Price > 5 && w.Price <= 20);
      int removedQty = itemsToRemove.Count();

      _biggyMemoryList.Remove(itemsToRemove.ToList());

      var storeWidgets = _widgetStore.Load();
      Assert.True(_biggyMemoryList.Count() < storeWidgets.Count());
    }


    [Fact(DisplayName = "Initializes In-Memory Biggy List with no store")]
    public void Initializes_List_With_No_Store() {
      _biggyMemoryList = new BiggyList<Widget>(null);
      var batch = new List<Widget>();
      for (int i = 0; i < INSERT_QTY; i++) {
        batch.Add(new Widget { SKU = string.Format("00{0}", i), Name = string.Format("Test widget {0}", i), Price = i });
      }
      _biggyMemoryList.Add(batch);

      Assert.True(_biggyMemoryList.Count() == INSERT_QTY);
    }


    [Fact(DisplayName = "Initializes JsonStore when no Ctor Arguments are passed")]
    public void Initializes_List_With_Json_Default_Store() {
      _biggyMemoryList = new BiggyList<Widget>();
      _biggyMemoryList.Clear();
      var batch = new List<Widget>();
      for (int i = 0; i < INSERT_QTY; i++) {
        batch.Add(new Widget { SKU = string.Format("00{0}", i), Name = string.Format("Test widget {0}", i), Price = i });
      }
      _biggyMemoryList.Add(batch);
      Assert.True(_biggyMemoryList.Count() == INSERT_QTY);
    }

    [Fact(DisplayName = "Initializes in-Memory only when no only bool is passed as true")]
    public void Initializes_Memory_Only_List_With_True_Argument() {
      _biggyMemoryList = new BiggyList<Widget>(inMemory: true);
      var BiggyJsonList = new BiggyList<Widget>();
      _biggyMemoryList.Clear();
      BiggyJsonList.Clear();

      var batch = new List<Widget>();
      for (int i = 0; i < INSERT_QTY; i++) {
        batch.Add(new Widget { SKU = string.Format("00{0}", i), Name = string.Format("Test widget {0}", i), Price = i });
      }
      _biggyMemoryList.Add(batch);
      BiggyJsonList.Add(batch);

      int memoryCount = _biggyMemoryList.Count();
      _biggyMemoryList.Clear();

      BiggyJsonList = new BiggyList<Widget>();

      Assert.True(memoryCount == INSERT_QTY && _biggyMemoryList.Count() == 0 && BiggyJsonList.Count() == INSERT_QTY);
    }
  }
}
