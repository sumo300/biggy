using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Biggy;
using Biggy.JSON;

namespace Tests.JSON {

  [Trait("JSON Store", "")]
  public class Store {
    IBiggyStore<Widget> _widgets;

    public Store() {
      _widgets = new JsonStore<Widget>(dbName: "widgets");
    }

    [Fact(DisplayName = "Initializes Store")]
    public void Initializes_Typed_Store() {
      _widgets.Clear();
      Assert.True(_widgets != null);
    }

    [Fact(DisplayName = "Adds an item to Store")]
    public void Adds_Item_To_Store() {
      _widgets.Clear();
      _widgets.Add(new Widget { SKU = "001", Name = "Test widget 1", Price = 2.00M });
      var itemsInStore = _widgets.Load();
      Assert.True(itemsInStore.Count() > 0);
    }

    [Fact(DisplayName = "Updates an item in the Store")]
    public void Updates_Item_In_Store() {
      _widgets.Clear();
      _widgets.Add(new Widget { SKU = "001", Name = "Test widget 1", Price = 2.00M });
      var updateMe = _widgets.Load().FirstOrDefault();
      updateMe.Name = "UPDATED";
      _widgets.Update(updateMe);

      var updated = _widgets.Load().FirstOrDefault();
      
      Assert.True(updated.Name == "UPDATED");
    }

    [Fact(DisplayName = "Removes an item from the Store")]
    public void Removes_Item_From_Store() {
      _widgets.Clear();
      _widgets.Add(new Widget { SKU = "001", Name = "Test widget 1", Price = 2.00M });
      var removeMe = _widgets.Load().FirstOrDefault();
      _widgets.Remove(removeMe);
      var remainingWidgets = _widgets.Load();
      Assert.True(remainingWidgets.Count() == 0);
    }

    [Fact(DisplayName = "Adds batch of items to Store")]
    public void Adds_Batch_Of_Items_To_Store() {
      _widgets.Clear();

      int INSERT_QTY = 100;
      var batch = new List<Widget>();
      for (int i = 0; i < INSERT_QTY; i++) {
        batch.Add(new Widget { SKU = string.Format("00{0}", i), Name = string.Format("Test widget {0}", i), Price = 2.00M });
      }
      _widgets.Add(batch);
      var itemsInStore = _widgets.Load();
      Assert.True(itemsInStore.Count() == INSERT_QTY);
    }

    [Fact(DisplayName = "Removes Range of items from Store")]
    public void Removes_Range_From_Store() {
      _widgets.Clear();

      int INSERT_QTY = 100;
      var batch = new List<Widget>();
      for (int i = 0; i < INSERT_QTY; i++) {
        batch.Add(new Widget { SKU = string.Format("00{0}", i), Name = string.Format("Test widget {0}", i), Price = 2.00M + i });
      }
      _widgets.Add(batch);
      var itemsToRemove = _widgets.Load().Where(w => w.Price > 5 && w.Price <= 20);
      int removedQty = itemsToRemove.Count();
      _widgets.Remove(itemsToRemove.ToList());
      var remaining = _widgets.Load();
      Assert.True(removedQty > 0 && removedQty < INSERT_QTY && remaining.Count() == (INSERT_QTY - removedQty));
    }

  }
}
