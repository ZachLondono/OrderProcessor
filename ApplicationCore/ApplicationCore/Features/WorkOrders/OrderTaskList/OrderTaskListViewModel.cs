using ApplicationCore.Features.Shared;
using ApplicationCore.Features.WorkOrders.Shared;
using ApplicationCore.Features.WorkOrders.Shared.Commands;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Infrastructure.UI;
using Blazored.Modal.Services;

namespace ApplicationCore.Features.WorkOrders.OrderTaskList;

public class OrderTaskListViewModel {

    public Action? OnPropertyChanged { get; set; }

    public Guid? OrderId { get; set; }

    #region ObservableProperties

    private bool _isLoading = false;
    public bool IsLoading {
        get => _isLoading;
        set {
            _isLoading = value;
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

    private List<WorkOrder> _workOrders = new();
    public IEnumerable<WorkOrder> WorkOrders {
        get => _workOrders;
        private set {
            _workOrders = new(value);
            OnPropertyChanged?.Invoke();
        }
    }

    #endregion

    private readonly IBus _bus;
    private readonly IUIBus _uiBus;
    private readonly IModalService _modalService;

    public OrderTaskListViewModel(IBus bus, IUIBus uiBus, IModalService modalService) {
        _bus = bus;
        _uiBus = uiBus;
        _modalService = modalService;
    }

    public async Task LoadWorkOrders() {

        if (IsLoading) return;

        IsLoading = true;
        if (OrderId is null) {
            Error = "No order selected";
            IsLoading = false;
            return;
        }

        var result = await _bus.Send(new GetWorkOrdersInOrder.Query((Guid)OrderId));

        result.Match(

            workorders => {
                WorkOrders = workorders;
            },

            error => {
                Error = $"[{error.Title}] : {error.Details}";
            }

        );

        IsLoading = false;

    }

    public async Task CompleteWorkOrder(WorkOrder workOrder) {

        var result = await _bus.Send(new UpdateWorkOrder.Command(workOrder.Id, workOrder.Name, Status.Complete));

        await result.MatchAsync(

            async _ => {
                workOrder.Status = Status.Complete;
                await _modalService.OpenInformationDialog("Updated", $"Work order '{workOrder.Name}' completed", InformationDialog.MessageType.Information);
                OnPropertyChanged?.Invoke();
            },

            _modalService.OpenErrorDialog

        );

        _uiBus.Publish(new WorkOrdersUpdatNotification());

    }

    public async Task DeleteWorkOrder(WorkOrder workOrder) {

        var result = await _bus.Send(new DeleteWorkOrder.Command(workOrder.Id));

        await result.MatchAsync(

            async _ => {
                _workOrders.Remove(workOrder);
                await _modalService.OpenInformationDialog("Deleted", $"Work order '{workOrder.Name}' deleted", InformationDialog.MessageType.Information);
            },

            _modalService.OpenErrorDialog

        );

        OnPropertyChanged?.Invoke();
        _uiBus.Publish(new WorkOrdersUpdatNotification());

    }

}
