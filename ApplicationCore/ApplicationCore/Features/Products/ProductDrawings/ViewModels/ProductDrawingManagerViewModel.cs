using ApplicationCore.Features.Orders.ProductDrawings.Commands;
using ApplicationCore.Features.Orders.ProductDrawings.Models;
using ApplicationCore.Features.Orders.ProductDrawings.Queries;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Features.Orders.ProductDrawings.ViewModels;

public class ProductDrawingManagerViewModel {

    private readonly IBus _bus;
    private Guid _productId;

    public Action? OnPropertyChanged { get; set; }
    public List<ProductDrawingRowModel> DrawingRows { get; set; } = new();

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

    public ProductDrawingManagerViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task Loaded(Guid productId) {

        try {

            var response = await _bus.Send(new GetProductDrawings.Query(productId));
            response.Match(
                drawings => {
                    DrawingRows = drawings.Select(drawing => new ProductDrawingRowModel(drawing)).ToList();
                    _productId = productId;
                },
                error => _error = error
            );

        } catch (Exception ex) {

            Error = new() {
                Title = "Failed to Load Product's Drawings",
                Details = ex.Message
            };

        }

    }

    public async Task RemoveDrawing(ProductDrawingRowModel row) {

        IsLoading = true;

        try {

            var response = await _bus.Send(new DeleteProductDrawing.Command(row.Drawing.Id));
            response.Match(
                _ => {
                    DrawingRows.Remove(row);
                    Error = null;
                }, 
                error => Error = error
            );

        } catch (Exception ex) {

            Error = new() {
                Title = "Failed to Remove Drawing",
                Details = ex.Message
            };

        }

        IsLoading = false;

    }

    public string GetNewDrawingName() {
        int num = 0;
        string name = "New Drawing";
        while (DrawingRows.Any(r => r.Name == name)) {
            name = $"New Drawing ({++num})";
        }

        return name;
    }

    public async Task AddActiveDrawing() {

        IsLoading = true;

        try {

            string name = GetNewDrawingName();
    
            var drawing = new ProductDrawing() {
                Id = Guid.NewGuid(),
                ProductId = _productId,
                Name = name,
                DXFData = Array.Empty<byte>(),
            };
    
            var saveResponse = await _bus.Send(new SaveActiveDrawingToProduct.Command(drawing.ProductId, drawing.Id, name));
    
            saveResponse.Match(
                _ => {
                    DrawingRows.Add(new(drawing));
                    Error = null;
                },
                e => Error = e);
    
        } catch (Exception ex) {

            Error = new() {
                Title = "Failed to Add Active Drawing to Product",
                Details = ex.Message
            };

        }

        IsLoading = false;

    }

}
