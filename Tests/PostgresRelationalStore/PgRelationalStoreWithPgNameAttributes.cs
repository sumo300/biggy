using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using Biggy.Postgres;
using Biggy.Core;

namespace Tests {
  [TestFixture()]
  [Category("PG Relational Store")]
  public class PgRelationalStoreWithNameAttributes {

    pgDbCore _db;

    [SetUp]
    public void init() {
      _db = new pgDbCore("biggy_test");
      DropCreateTestTables();
    }

    void DropCreateTestTables() {
      string WorkOrderTableSql = ""
        + "CREATE TABLE \"wk_order\" ( \"wo_id\" serial NOT NULL, \"desc\" text, CONSTRAINT pk_wk_order_wo_id PRIMARY KEY (\"wo_id\"))";
      _db.TryDropTable("wk_order");
      _db.TransactDDL(WorkOrderTableSql);     
    }

    [Test()]
    public void Relational_Store_Inserts_record_with_name_attributes() {
      var WorkOrderStore = new pgRelationalStore<WorkOrder>(_db);
      var newWorkOrder = new WorkOrder { Description = "Take out the Trash" };
      WorkOrderStore.Add(newWorkOrder);

      var foundWorkOrder = WorkOrderStore.TryLoadData().FirstOrDefault();
      Assert.IsTrue(foundWorkOrder != null && foundWorkOrder.WorkOrderId == 1);
    }

    [Test()]
    public void Relational_Store_Inserts_range_of_records_with_name_attributes() {
      var WorkOrderStore = new pgRelationalStore<WorkOrder>(_db);
      var myBatch = new List<WorkOrder>();
      int qtyToAdd = 10;
      for (int i = 1; i <= qtyToAdd; i++) {
        var newWorkOrder = new WorkOrder { Description = "Replace Lightbulbs " + i };
        myBatch.Add(newWorkOrder);
      }
      WorkOrderStore.Add(myBatch);
      var workOrders = WorkOrderStore.TryLoadData();
      Assert.IsTrue(workOrders.Count == qtyToAdd);
    }

    [Test()]
    public void Relational_Store_Updates_record_with_name_attributes() {
      var WorkOrderStore = new pgRelationalStore<WorkOrder>(_db);
      var newWorkOrder = new WorkOrder { Description = "Snake toilet" };
      WorkOrderStore.Add(newWorkOrder);

      // Now go fetch the record again and update:
      string newValue = "Update: Call Roto-Rooter";
      var foundWorkOrder = WorkOrderStore.TryLoadData().FirstOrDefault();
      foundWorkOrder.Description = newValue;
      WorkOrderStore.Update(foundWorkOrder);

      foundWorkOrder = WorkOrderStore.TryLoadData().FirstOrDefault();
      Assert.IsTrue(foundWorkOrder != null && foundWorkOrder.Description == newValue);
    }

    [Test()]
    public void Relational_Store_Updates_range_of_records_with_name_attributes() {
      var WorkOrderStore = new pgRelationalStore<WorkOrder>(_db);
      var myBatch = new List<WorkOrder>();
      int qtyToAdd = 10;
      for (int i = 1; i <= qtyToAdd; i++) {
        var newWorkOrder = new WorkOrder { Description = "Caulk Tub " + i };
        myBatch.Add(newWorkOrder);
      }
      WorkOrderStore.Add(myBatch);

      // Re-load, and update:
      var workOrders = WorkOrderStore.TryLoadData();
      for (int i = 0; i < qtyToAdd; i++) {
        workOrders.ElementAt(i).Description = "Updated Tubs" + i;
      }
      WorkOrderStore.Update(workOrders);

      // Reload, and check updated names:
      workOrders = WorkOrderStore.TryLoadData().Where(c => c.Description.Contains("Updated")).ToList();
      Assert.IsTrue(workOrders.Count == qtyToAdd);
    }

    [Test()]
    public void Relational_Store_Deletes_record_with_name_attributes() {
      var WorkOrderStore = new pgRelationalStore<WorkOrder>(_db);
      var newWorkOrder = new WorkOrder { Description = "Eradicate bed bugs" };
      WorkOrderStore.Add(newWorkOrder);

      // Load from back-end:
      var workOrders = WorkOrderStore.TryLoadData();
      int qtyAdded = workOrders.Count;

      // Delete:
      var foundWorkOrder = workOrders.FirstOrDefault();
      WorkOrderStore.Delete(foundWorkOrder);

      int remaining = WorkOrderStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == 1 && remaining == 0);
    }

    [Test()]
    public void Relational_Store_Deletes_range_of_records_with_name_attributes() {
      var WorkOrderStore = new pgRelationalStore<WorkOrder>(_db);
      var myBatch = new List<WorkOrder>();
      int qtyToAdd = 10;
      for (int i = 0; i < qtyToAdd; i++) {
        var newWorkOrder = new WorkOrder { Description = "Kill mice dead " + i };
        myBatch.Add(newWorkOrder);
      }
      WorkOrderStore.Add(myBatch);

      // Re-load from back-end:
      var workOrders = WorkOrderStore.TryLoadData();
      int qtyAdded = workOrders.Count;

      // Select 5 for deletion:
      int qtyToDelete = 5;
      var deleteThese = workOrders.Where(c => c.WorkOrderId <= qtyToDelete);

      // Delete:
      WorkOrderStore.Delete(deleteThese);
      int remaining = WorkOrderStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == qtyAdded - qtyToDelete);
    }

    [Test()]
    public void Relational_Store_Deletes_all_records_with_name_attributes() {
      var WorkOrderStore = new pgRelationalStore<WorkOrder>(_db);
      var myBatch = new List<WorkOrder>();
      int qtyToAdd = 10;
      for (int i = 0; i < qtyToAdd; i++) {
        var newWorkOrder = new WorkOrder { Description = "Kill Roaches dead " + i };
        myBatch.Add(newWorkOrder);
      }
      WorkOrderStore.Add(myBatch);

      // Re-load from back-end:
      var workOrders = WorkOrderStore.TryLoadData();
      int qtyAdded = workOrders.Count;

      // Delete:
      WorkOrderStore.DeleteAll();
      int remaining = WorkOrderStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == 0);
    }
  }
}

