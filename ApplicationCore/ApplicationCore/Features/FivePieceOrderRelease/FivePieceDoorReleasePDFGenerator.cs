using ApplicationCore.Features.OptimizeStrips;
using Domain.Orders.Entities.Products.Doors;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using OrderExporting.FivePieceDoorCutList;
using OrderExporting.Shared;

namespace ApplicationCore.Features.FivePieceOrderRelease;

public class FivePieceDoorReleasePDFGenerator(FivePieceDoorCutListWriter writer) {

    private readonly FivePieceDoorCutListWriter _writer = writer;

    public string ReleaseOrder(FivePieceOrder order, string outputDirectory) {

        var frameThickness = Dimension.FromMillimeters(19);
        var panelThickness = Dimension.FromMillimeters(6.35);
        var material = order.Material;

        var doors = order.LineItems
                         .Select(i => new FivePieceDoorProduct(Guid.NewGuid(),
                                                  i.Qty,
                                                  0,
                                                  i.PartNum,
                                                  "",
                                                  i.Width,
                                                  i.Height,
                                                  new DoorFrame() {
                                                      LeftStile = i.LeftStile,
                                                      RightStile = i.RightStile,
                                                      TopRail = i.TopRail,
                                                      BottomRail = i.BottomRail,
                                                  },
                                                  frameThickness,
                                                  panelThickness,
                                                  material,
                                                  DoorType.Door) {
                             ProductionNotes = [i.SpecialFeatures]
                         })
                         .ToArray();

        CreateCutList(order, outputDirectory, material, doors);
        CreateOptimizedCutList(doors);

        throw new NotImplementedException();

    }

    private static void CreateOptimizedCutList(FivePieceDoorProduct[] doors) {

        var kerf = 4;
        var stripLength = 2438;

        var partsByWidth = doors.SelectMany(d => d.GetParts()).GroupBy(p => (p.Width, p.Material));
        List<IDocumentDecorator> results = [];

        var optimizer = new PartOptimizer();

        foreach (var group in partsByWidth) {

            var width = group.Key;
            var lengths = group.Select(g => g.Length.AsMillimeters()).ToArray();

            var result = optimizer.OptimizeStrips(stripLength, kerf, lengths);

            results.Add(new OptimizationDocumentDecorator() {
                Material = group.Key.Material,
                StripWidth = group.Key.Width.AsMillimeters(),
                Optimization = result,
                StripLength = stripLength
            });

        }

    }

    private void CreateCutList(FivePieceOrder order, string outputDirectory, string material, FivePieceDoorProduct[] doors) {

        var cutList = new FivePieceCutList() {
            Material = material,
            CustomerName = order.CompanyName,
            VendorName = "",
            Note = "",
            OrderDate = order.OrderDate,
            OrderName = order.JobName,
            OrderNumber = order.TrackingNumber,
            TotalDoorCount = doors.Length,
            Items = doors.SelectMany(d => {

                return d.GetParts()
                        .Select(p => new FivePieceDoorLineItem() {
                            CabNumber = d.ProductNumber,
                            Qty = d.Qty,
                            Note = d.ProductionNotes.FirstOrDefault() ?? "",
                            PartName = p.Name,
                            Width = p.Width.AsMillimeters(),
                            Length = p.Length.AsMillimeters()
                        });

            })
        };

        var result = _writer.WriteCutList(cutList, outputDirectory, false);

    }

}
