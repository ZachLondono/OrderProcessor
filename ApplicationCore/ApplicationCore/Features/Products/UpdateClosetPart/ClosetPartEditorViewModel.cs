using ApplicationCore.Features.Orders.Shared.Domain.Products.Closets;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Features.Products.UpdateClosetPart;

public class ClosetPartEditorViewModel {

    private readonly IBus _bus;

    public Action? OnPropertyChanged { get; set; }
    public Func<Task>? CloseAsync { get; set; }

    // TODO: load a model specifically for the editor
    public ClosetPart? Product { get; set; }

    private Error? error;
    public Error? Error {
        get => error;
        set {
            error = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public ClosetPartEditorViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task Update() {

        if (_bus is null || Product is null) return;

        Error = null;

        try {

            var response = await _bus.Send(new UpdateClosetPart.Command(Product));

            await response.Match(
                unit => CloseAsync?.Invoke() ?? Task.CompletedTask,
                error => {
                    Error = error;
                    return Task.CompletedTask;
                });

        } catch (Exception ex) {

            Error = new() {
                Title = "Failed to Update Closet Part",
                Details = ex.Message
            };

        }

    }

}
