using System.Collections.Generic;
using System.Linq;
using Biggy;
using Xunit;

namespace Tests
{
    [Trait("BiggyList with no store", "")]
    public class BiggyListNoStoreTests
    {
        private readonly BiggyList<Widget> _biggyList;

        public BiggyListNoStoreTests()
        {
            _biggyList = new BiggyList<Widget>(null, inMemory: true);
        }

        [Fact(DisplayName = "Initializes Store with empty list")]
        public void Initializes_Typed_Store_With_Empty_List()
        {
            Assert.True(_biggyList.ToList() != null);
        }

        [Fact(DisplayName = "Adds an item")]
        public void Adds_Item_To_Store()
        {
            _biggyList.Add(new Widget { SKU = "001", Name = "Test widget 1", Price = 2.00M });
            var item = _biggyList.FirstOrDefault(x => x.SKU == "001");

            Assert.NotNull(item);
        }

        [Fact(DisplayName = "Updates an item")]
        public void Updates_Item_In_List()
        {
            _biggyList.Add(new Widget { SKU = "001", Name = "Test widget 1", Price = 2.00M });
            var updateMe = _biggyList.First(x => x.SKU == "001");
            updateMe.Name = "UPDATED";
            var updated = _biggyList.First(x => x.SKU == "001");

            Assert.True(updated.Name == "UPDATED");
        }

        [Fact(DisplayName = "Removes an item")]
        public void Removes_Item_From_List()
        {
            _biggyList.Add(new Widget { SKU = "001", Name = "Test widget 1", Price = 2.00M });
            var removeMe = _biggyList.First(x => x.SKU == "001");
            _biggyList.Remove(removeMe);

            Assert.Empty(_biggyList);
        }

        [Fact(DisplayName = "Removes Range of items")]
        public void Removes_Range_From_List()
        {
            int INSERT_QTY = 100;
            var batch = new List<Widget>();
            for (int i = 0; i < INSERT_QTY; i++)
            {
                batch.Add(new Widget { SKU = string.Format("00{0}", i), Name = string.Format("Test widget {0}", i), Price = 2.00M + i });
            }
            _biggyList.Add(batch);

            var itemsToRemove = _biggyList.Where(w => w.Price > 5 && w.Price <= 20).ToList();
            int removedQty = itemsToRemove.Count;
            _biggyList.Remove(itemsToRemove);
            var remaining = _biggyList.ToList();

            Assert.True(removedQty > 0 && removedQty < INSERT_QTY && remaining.Count == (INSERT_QTY - removedQty));
        }
    }
}
