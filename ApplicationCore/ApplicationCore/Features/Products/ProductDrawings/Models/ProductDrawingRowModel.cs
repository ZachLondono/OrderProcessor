namespace ApplicationCore.Features.Orders.ProductDrawings.Models;

public class ProductDrawingRowModel {

    public string Name { get; set; }

    public ProductDrawing Drawing { get; set; }

    public bool IsBeingEdited { get; set; }

    public bool IsBeingDeleted { get; set; }

    public ProductDrawingRowModel(ProductDrawing drawing) {
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
