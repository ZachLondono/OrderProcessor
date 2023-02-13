using ApplicationCore.Features.Shared;
using ApplicationCore.Infrastructure;
using DocumentFormat.OpenXml.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Features.WorkOrders;

internal class BarCodeScanningDialogViewModel {

    public string Code { get; set; } = string.Empty;

    public Action? OnPropertyChanged;

    private string? _message = null;
    public string? Message {
        get => _message;
        set {
            _message = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private string? _error = null;
    public string? Error {
        get => _error;
        set {
            _error = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public readonly IBus _bus;

    public BarCodeScanningDialogViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task SubmitScannedBarcode() {

        Error = null;
        Message = null;

        Guid? workOrderId = null;

        try {

            workOrderId = ShortGuid.Parse(Code).ToGuid();

        } catch {

            Error = "Barcode foramt is invalid";
            return;

        }

        var queryResponse = await _bus.Send(new GetWorkOrderById.Query((Guid)workOrderId));

        WorkOrder? workOrder = null;
        queryResponse.OnSuccess(wo => workOrder = wo);

        if (workOrder is null) {
            Error = $"Could not find work order";
            return;
        }

        var commandResponse = await _bus.Send(new UpdateWorkOrder.Command((Guid)workOrderId, workOrder.Name, Status.Complete));

        commandResponse.Match(
            _ => Message = $"Work order '{workOrder.Name}' updated.",
            error => Error = $"{error.Title} - {error.Details}"
        );

    }

}