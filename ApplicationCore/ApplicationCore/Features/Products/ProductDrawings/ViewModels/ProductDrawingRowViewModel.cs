using ApplicationCore.Features.Orders.ProductDrawings.Commands;
using ApplicationCore.Features.Orders.ProductDrawings.Models;
using Domain.Infrastructure.Bus;

namespace ApplicationCore.Features.Orders.ProductDrawings.ViewModels;

public class ProductDrawingRowViewModel {

    private readonly IBus _bus;

    public Action? OnPropertyChanged { get; set; }

    private Error? _error = null;
    public Error? Error {
        get => _error;
        set {
            _error = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private bool _isLoading = false;
    public bool IsLoading {
        get => _isLoading;
        set {
            _isLoading = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public ProductDrawingRowViewModel(IBus bus) {
        _bus = bus;
    }

    public void StartDeletingDrawing(ProductDrawingRowModel row) {
        row.Delete();
        OnPropertyChanged?.Invoke();
    }

    public void CancelDeletingDrawing(ProductDrawingRowModel row) {
        row.Reset();
        OnPropertyChanged?.Invoke();
    }

    public void StartEditingDrawing(ProductDrawingRowModel row) {
        row.Edit();
        OnPropertyChanged?.Invoke();
    }

    public void CancelEditingDrawing(ProductDrawingRowModel row) {
        row.Reset();
        OnPropertyChanged?.Invoke();
    }

    public async Task ImportIntoDocument(ProductDrawing drawing, ImportProductDrawingIntoActiveDocument.ImportMode mode) {

        IsLoading = true;

        await Task.Delay(1000);

        try {

            var response = await _bus.Send(new ImportProductDrawingIntoActiveDocument.Command(drawing, mode));

            response.Match(
                _ => Error = null,
                e => Error = e);

        } catch (Exception ex) {

            Error = new() {
                Title = "Error Importing Drawing into Active Document",
                Details = ex.Message
            };

        }

        IsLoading = false;

    }

    public async Task UpdateDrawingFromDocument(ProductDrawing drawing) {

        IsLoading = true;

        try {

            var saveResponse = await _bus.Send(new SaveActiveDrawingToProduct.Command(drawing.ProductId, drawing.Id, drawing.Name));

            saveResponse.Match(
                _ => Error = null,
                e => Error = e);

        } catch (Exception ex) {

            Error = new() {
                Title = "Failed to Update Drawing From Active Document",
                Details = ex.Message
            };

        }

        IsLoading = false;

    }

    public async Task UpdateDrawingName(ProductDrawingRowModel row) {

        IsLoading = true;

        try {

            var response = await _bus.Send(new UpdateProductDrawingName.Command(row.Drawing.Id, row.Name));
            response.Match(
                _ => {
                    row.Commit();
                    Error = null;
                },
                error => Error = error
            );

        } catch (Exception ex) {

            Error = new() {
                Title = "Failed to Save Changes",
                Details = ex.Message
            };

        }

        IsLoading = false;

    }

}
