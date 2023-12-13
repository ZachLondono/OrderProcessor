namespace ApplicationCore.Features.Orders.OrderLoading.LoadTailoredLivingSpreadsheetOrderData;

public record Item(string Description, string PartNumber, int Qty, double Width, double Height, double Depth, double Dim4, decimal Cost, decimal LaborCharge, decimal TotalCharge, string SpecialNotes);

