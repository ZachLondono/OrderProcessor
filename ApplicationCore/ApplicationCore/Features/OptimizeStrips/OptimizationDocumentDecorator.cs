using ApplicationCore.Features.FivePieceOrderRelease;
using OrderExporting.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApplicationCore.Features.OptimizeStrips;

public class OptimizationDocumentDecorator : IDocumentDecorator {

    public required PartOptimizer.OptimizationResult Optimization { get; set; }
    public required string Material { get; set; }
    public required double StripWidth { get; set; }
    public required double StripLength { get; set; }

    public void Decorate(IDocumentContainer container) {

        container.Page(page => {

            page.Size(PageSizes.Letter);
            page.Margin(30);

            page.Header()
                .AlignCenter()
                .Text($"{StripWidth}\" x {StripLength}\" - {Material}")
                .Bold()
                .FontSize(16);

            page.Content()
                .Column(col => {

                    col.Item().Text($"{Optimization.OptimizedStrips.Length} Lengths").Bold().FontSize(16);

                    int i = 1;
                    foreach (var part in Optimization.OptimizedStrips) {

                        string val = string.Join(";  ", part.Select(p => Math.Round(p, 1).ToString()));

                        col.Item()
                            .PaddingLeft(20)
                            .Text($"{i++} - {val}");

                    }

                    if (Optimization.UnplacedParts.Length != 0) {

                        col.Item()
                            .PaddingTop(30)
                            .Text("Unplaced Items")
                            .Bold()
                            .FontSize(16);

                        foreach (var part in Optimization.UnplacedParts) {
                            col.Item()
                                .PaddingLeft(20)
                                .Text(Math.Round(part).ToString());
                        }

                    }

                });

        });

    }

}
