using ApplicationCore.Pages.CustomerDetails;
using Domain.Companies.Entities;
using OrderLoading.LoadHafeleMDFDoorSpreadsheetOrderData.ReadOrderFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Features.MDFDoorOrders.ProcessHafeleMDFOrder;

public class HafeleMDFDoorOrderProcessor {

    public async Task ProcessOrderAsync(ProcessOptions options) {

        var orderData = LoadOrderData(options.DataFile);

        if (orderData is null) {

            // Could not load order data
            return;

        }

        if (options.GenerateInvoice) {
            var invoice = GenerateInvoice();
            if (options.SendInvoiceEmail) {
                SendInvoiceEmail();
            }
        }

        if (options.FillOrderSheet) {
            FillOrderSheet();
        }

        if (options.PostToGoogleSheets) {
            await PostOrderToGoogleSheet(orderData);
        }

    }

    private HafeleMDFDoorOrder? LoadOrderData(string orderData) {
        var result = HafeleMDFDoorOrder.Load(orderData);
        //result.Warnings
        //result.Errors
        return result.Data;
    }

    private string GenerateInvoice() => throw new NotImplementedException();

    private string SendInvoiceEmail() => throw new NotImplementedException();

    private string FillOrderSheet() => throw new NotImplementedException();

    private static async Task PostOrderToGoogleSheet(HafeleMDFDoorOrder order) {

        var row = new GoogleSheetRow() {
            FileDate = order.Options.Date.ToShortDateString(),
            HafelePO = order.Options.HafelePO,
            ProjectNumber = order.Options.HafeleOrderNumber,
            ConfigNumber = "",
            CustomerName = order.Options.Company,
            CustomerPO = order.Options.JobName,
            TotalItemCount = order.Sizes.Sum(s => s.Qty).ToString(),
            ShipDate = order.Options.GetDueDate().ToShortDateString(),
            InvoiceAmount = order.GetInvoiceAmount().ToString(),
            TrackingNumber = ""
        };

        await row.PostData();

    }

}
