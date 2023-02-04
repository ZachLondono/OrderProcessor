using ApplicationCore.Features.Shared;
using ApplicationCore.Infrastructure;
using Blazored.Modal.Services;
using static ApplicationCore.Features.WorkOrders.AllWorkOrders.GetAllWorkOrders;

namespace ApplicationCore.Features.WorkOrders.AllWorkOrders;

public class AllWorkOrdersListViewModel {

    public Action? OnPropertyChanged { get; set; }

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

    private List<Model> _workOrders = new();
    public IEnumerable<Model> WorkOrders {
        get => _workOrders;
        private set {
            _workOrders = new(value);
            OnPropertyChanged?.Invoke();
        }
    }

    #endregion

    private readonly IBus _bus;
    private readonly IModalService _modalService;

    public AllWorkOrdersListViewModel(IBus bus, IModalService modalService) {
        _bus = bus;
        _modalService = modalService;
    }

    public async Task LoadWorkOrders() {

        if (IsLoading) return;
        IsLoading = true;

        var result = await _bus.Send(new Query());

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

    public async Task CompleteWorkOrder(Model workOrder) {

        var result = await _bus.Send(new UpdateWorkOrder.Command(workOrder.Id, workOrder.Name, Status.Complete));

        await result.MatchAsync(

            _ => {
                workOrder.Status = Status.Complete;
                OnPropertyChanged?.Invoke();
                return Task.CompletedTask;
            },

            _modalService.OpenErrorDialog

        );

    }

    public async Task DeleteWorkOrder(Model workOrder) {

        var result = await _bus.Send(new DeleteWorkOrder.Command(workOrder.Id));

        await result.MatchAsync(

            _ => {
                _workOrders.Remove(workOrder);
                OnPropertyChanged?.Invoke();
                return Task.CompletedTask;
            },

            _modalService.OpenErrorDialog

        );

    }

}
