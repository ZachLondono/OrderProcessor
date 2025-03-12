using Domain.Infrastructure.Bus;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace ApplicationCore.Features.Products.UpdateClosetPart;

public class ClosetPartEditorViewModel {

    private readonly ILogger<ClosetPartEditorViewModel> _logger;
    private readonly IBus _bus;

    public Action? OnPropertyChanged { get; set; }
    public Func<Task>? CloseAsync { get; set; }

    // TODO: load a model specifically for the editor
    public ClosetPartEditModel? EditModel { get; set; }

    private bool _isUpdating;
    public bool IsUpdating {
        get => _isUpdating;
        set {
            _isUpdating = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private Error? error;
    public Error? Error {
        get => error;
        set {
            error = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public ClosetPartEditorViewModel(ILogger<ClosetPartEditorViewModel> logger, IBus bus) {
        _logger = logger;
        _bus = bus;
    }

    public async Task Update() {

        if (_bus is null || EditModel is null) return;

        IsUpdating = true;
        Error = null;

        using (LogContext.PushProperty("EditModel", EditModel, true)) {


            try {

                var response = await _bus.Send(new UpdateClosetPart.Command(EditModel.ToProduct()));

                await response.Match(
                    unit => CloseAsync?.Invoke() ?? Task.CompletedTask,
                    error => {
                        _logger.LogError("An error was returned from the UpdateClosetPart command while trying to update a closet part from the closet part editor. {Error}", Error);
                        Error = error;
                        return Task.CompletedTask;
                    });

            } catch (Exception ex) {

                _logger.LogError(ex, "Exception thrown by the UpdateClosetPart command while trying to update a closet part from the closet part editor.");
                Error = new() {
                    Title = "Failed to Update Closet Part",
                    Details = ex.Message
                };

            }

        }

        IsUpdating = false;

    }

    public void AddAskParameter() {
        EditModel?.AskParameters.Add(new() {
            Name = "",
            Value = ""
        });
        OnPropertyChanged?.Invoke();
    }

    public void RemoveAskParameter(AskParameter parameter) {
        EditModel?.AskParameters.Remove(parameter);
        OnPropertyChanged?.Invoke();
    }

}