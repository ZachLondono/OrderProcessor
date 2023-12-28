using ApplicationCore.Features.AllmoxyOrderExport.Products;
using ApplicationCore.Features.AllmoxyOrderExport.Products.FloorMountedVerticals;
using ApplicationCore.Features.AllmoxyOrderExport.Products.Miscellaneous;
using ApplicationCore.Features.AllmoxyOrderExport.Products.Shelves;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace ApplicationCore.Features.AllmoxyOrderExport;

public class CSVOrderWriter() {

    public static Task WriteCSVOrder(IEnumerable<IAllmoxyProduct> products, string filePath) {

        if (!products.Any()) {
            throw new InvalidOperationException("No products to write");
        }

        CsvConfiguration config = new(CultureInfo.InvariantCulture) {
            HasHeaderRecord = false
        };

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, config);

        WriteRecords(products, csv);

        return Task.CompletedTask;

    }

    private static void WriteRecords(IEnumerable<IAllmoxyProduct> products, CsvWriter csv) {

        foreach (var product in products) {

            if (product is AdjustableShelf adjShelf) {
                csv.WriteRecord(adjShelf);
            } else if (product is FixedShelf fixedShelf) {
                csv.WriteRecord(fixedShelf);
            } else if (product is AdjustableSafetyShelf safetyShelf) {
                csv.WriteRecord(safetyShelf);
            } else if (product is DividerShelf dividerShelf) {
                csv.WriteRecord(dividerShelf);
            } else if (product is FixedLShapedShelf fixedLShapedShelf) {
                csv.WriteRecord(fixedLShapedShelf);
            } else if (product is AdjustableLShapedShelf adjustableLShapedShelf) {
                csv.WriteRecord(adjustableLShapedShelf);
            } else if (product is FixedAngledShelf fixedAngledShelf) {
                csv.WriteRecord(fixedAngledShelf);
            } else if (product is AdjustableAngledShelf adjustableAngledShelf) {
                csv.WriteRecord(adjustableAngledShelf);
            } else if (product is ShoeShelf shoeShelf) {
                csv.WriteRecord(shoeShelf);
            }

            else if (product is Back back) {
                csv.WriteRecord(back);
            } else if (product is Nailer nailer) {
                csv.WriteRecord(nailer);
            } else if (product is ToeKick toeKick) {
                csv.WriteRecord(toeKick);
            } else if (product is Top top) {
                csv.WriteRecord(top);
            }

            else if (product is FloorMountedPanels floorMountedPanel) {
                csv.WriteRecord(floorMountedPanel);
            } else if (product is IslandPanels islandPanels) {
                csv.WriteRecord(islandPanels);
            } else if (product is DividerPanels dividerPanels) {
                csv.WriteRecord(dividerPanels);
            } else if (product is FloorMountedTransitionPanels transitionPanels) {
                csv.WriteRecord(transitionPanels);
            } else if (product is FloorMountedPanelsMinimumHoles minimalHolePanels) {
                csv.WriteRecord(minimalHolePanels);
            } else if (product is FloorMountedPanelsFrontOrBackClip clipPanel) {
                csv.WriteRecord(clipPanel);
            } else if (product is FloorMountedPanelsCustomHeight customHeightPanel) {
                csv.WriteRecord(customHeightPanel);
            } else if (product is FloorMountedHutchPanels hutchPanel) {
                csv.WriteRecord(hutchPanel);
            }

            csv.NextRecord();

        }

        csv.Flush();

    }

}