namespace OrderExporting.Invoice;

public class CabinetPartItem {

    public int Line { get; set; }

    public int Qty { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

}