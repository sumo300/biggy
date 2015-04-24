using System;
using System.Linq;
using Biggy.Core;

namespace Tests {
  public class PropertyDocument {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
  }

  public class CompanyDocuments {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
  }

  public class WidgetDocuments {
    [PrimaryKey(Auto: false)]
    public int Identifier { get; set; }
    public string Category { get; set; }
  }

  public class GuitarDocuments {
    [PrimaryKey(Auto: false)]
    public string Sku { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
  }

  public class ErrorGuitarDocuments {
    [PrimaryKey(Auto: true)]
    public string Sku { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
  }

  public class InstrumentDocuments {
    public string Id { get; set; }
    public string Category { get; set; }
    public string Type { get; set; }
  }

    public class BowlingDocuments {
        [PrimaryKey(Auto: false)]
        public long Id { get; set; }
        public string Name { get; set; }
        public string Score { get; set; }
    }
}