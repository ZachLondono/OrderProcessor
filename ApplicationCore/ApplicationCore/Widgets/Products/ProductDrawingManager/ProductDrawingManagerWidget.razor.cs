using ApplicationCore.Features.BricsCAD;
using ApplicationCore.Features.Orders.ProductDrawings;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Services;
using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Widgets.Products.ProductDrawingManager;

public partial class ProductDrawingManagerWidget {

    [Parameter]
    public Guid ProductId { get; set; }

    [Inject]
    private IBus? Bus { get; set; } = null;

    private Error? _error = null;

    private bool _isLoading = false;
    public bool IsLoading {
        get => _isLoading;
        set {
            _isLoading = value;
            StateHasChanged();
        }
    }

    public List<ProductDrawingRow> DrawingRows { get; set; } = new();

    protected override async Task OnParametersSetAsync() {

        if (Bus is null) {
            return;
        }

        var response = await Bus.Send(new GetProductDrawings.Query(ProductId));

        response.Match(
            drawings => DrawingRows = drawings.Select(drawing => new ProductDrawingRow(drawing)).ToList(),
            error => _error = error
        );

    }

    private void StartDeletingDrawing(ProductDrawingRow row) {
        row.Delete();
        StateHasChanged();
    }

    private void CancelDeletingDrawing(ProductDrawingRow row) {
        row.Reset();
        StateHasChanged();
    }

    private async Task RemoveDrawing(ProductDrawingRow row) {

        if (Bus is null) {
            row.Reset();
            StateHasChanged();
            return;
        }

        IsLoading = true;

        var response = await Bus.Send(new DeleteProductDrawing.Command(row.Drawing.Id));
        DrawingRows.Remove(row);
        response.OnError(error => _error = error);

        IsLoading = false;

    }

    private async Task AddActiveDrawing() {

        if (Bus is null) {
            return;
        }

        IsLoading = true;

        var tmpPath = Path.GetTempPath() + Guid.NewGuid().ToString();

        var exportResponse = await Bus.Send(new ExportActiveDrawingDXFFile.Command(tmpPath));

        tmpPath += ".dxf";

        exportResponse.OnError(e => {
            _error = e;
            StateHasChanged();
        });

        if (exportResponse.IsError) {
            IsLoading = false;
            return;
        }

        var data = await File.ReadAllBytesAsync(tmpPath);
        File.Delete(tmpPath);

        var compressedData = CompressionService.Compress(data);

        string name = GetNewDrawingName();

        var drawing = new ProductDrawing() {
            Id = Guid.NewGuid(),
            ProductId = ProductId,
            DXFData = compressedData,
            Name = name
        };

        var saveResponse = await Bus.Send(new SaveProductDrawing.Command(drawing));

        saveResponse.OnError(e => {
            _error = e;
            StateHasChanged();
        });

        if (saveResponse.IsError) {
            IsLoading = false;
            return;
        }

        DrawingRows.Add(new(drawing));

        IsLoading = false;

    }

    private async Task UpdateDrawingFromDocument(ProductDrawing drawing) {

        if (Bus is null) {
            return;
        }

        IsLoading = true;

        var tmpPath = Path.GetTempPath() + Guid.NewGuid().ToString();

        var exportResponse = await Bus.Send(new ExportActiveDrawingDXFFile.Command(tmpPath));

        tmpPath += ".dxf";

        exportResponse.OnError(e => {
            _error = e;
            StateHasChanged();
        });

        if (exportResponse.IsError) {
            IsLoading = false;
            return;
        }

        var data = await File.ReadAllBytesAsync(tmpPath);
        File.Delete(tmpPath);

        var compressedData = CompressionService.Compress(data);

        drawing.DXFData = compressedData;

        var saveResponse = await Bus.Send(new SaveProductDrawing.Command(drawing));

        saveResponse.OnError(e => {
            _error = e;
            StateHasChanged();
        });

        if (saveResponse.IsError) {
            IsLoading = false;
            return;
        }

        IsLoading = false;

    }

    private string GetNewDrawingName() {
        int num = 0;
        string name = "New Drawing";
        while (DrawingRows.Any(r => r.Name == name)) {
            name = $"New Drawing ({++num})";
        }

        return name;
    }

    private async Task ImportIntoDocument(ProductDrawing drawing, ImportDXFIntoDrawing.ImportMode mode) {

        if (Bus is null) {
            return;
        }

        IsLoading = true;

        var uncompressed = CompressionService.Uncompress(drawing.DXFData);

        var dxfAscii = System.Text.Encoding.ASCII.GetString(uncompressed);

        var tmpFilePath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".dxf";

        await File.WriteAllTextAsync(tmpFilePath, dxfAscii);

        var response = await Bus.Send(new ImportDXFIntoDrawing.Command(tmpFilePath, mode));

        File.Delete(tmpFilePath);

        response.OnError(e => _error = e);

        IsLoading = false;

    }

    private void StartEditingDrawing(ProductDrawingRow row) {
        row.Edit();
        StateHasChanged();
    }

    private void CancelEditingDrawing(ProductDrawingRow row) {
        row.Reset();
        StateHasChanged();
    }

    private async Task SaveDrawingChanges(ProductDrawingRow row) {

        if (Bus is null) {
            row.Reset();
            return;
        }

        IsLoading = true;

        row.Commit();

        var response = await Bus.Send(new SaveProductDrawing.Command(row.Drawing));
        response.OnError(error => _error = error);

        IsLoading = false;

    }

    public class ProductDrawingRow {

        public string Name { get; set; }

        public ProductDrawing Drawing { get; set; }

        public bool IsBeingEdited { get; set; }

        public bool IsBeingDeleted { get; set; }

        public ProductDrawingRow(ProductDrawing drawing) {
            Name = drawing.Name;
            Drawing = drawing;
            IsBeingEdited = false;
        }

        public void Edit() {
            IsBeingEdited = true;
            IsBeingDeleted = false;
        }

        public void Delete() {
            IsBeingDeleted = true;
            IsBeingEdited = false;
        }

        public void Commit() {
            IsBeingEdited = false;
            IsBeingDeleted = false;
            Drawing.Name = Name;
        }

        public void Reset() {
            IsBeingDeleted = false;
            IsBeingEdited = false;
            Name = Drawing.Name;
        }

    }

}
