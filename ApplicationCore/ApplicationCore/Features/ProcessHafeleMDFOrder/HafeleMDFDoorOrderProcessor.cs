using OrderLoading.LoadHafeleMDFDoorSpreadsheetOrderData.ReadOrderFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Features.ProcessHafeleMDFOrder;

public class HafeleMDFDoorOrderProcessor {

    public void ProcessOrder(ProcessOptions options) {

        var orderData = LoadOrderData(options.OrderSheetTemplatePath);

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
            PostOrderToGoogleSheet();
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

    private string PostOrderToGoogleSheet() => throw new NotImplementedException();

}
