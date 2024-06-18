using ApplicationCore.Features.FivePieceOrderRelease;
using Domain.ValueObjects;
using OrderExporting.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApplicationCore.Features.OptimizeStrips;

public class OptimizationDocumentDecorator : IDocumentDecorator {

    public required PartOptimizer.OptimizationResult Optimization { get; set; }

    public void Decorate(IDocumentContainer container) {

        container.Page(page => {

            page.Size(PageSizes.Letter);
            page.Margin(30);

            page.Header()
                .AlignCenter()
                .Text($"{PartWidth.AsInchFraction()}\" x {MaterialLength.AsInchFraction()}\" - {Material}")
                .Bold()
                .FontSize(16);

            page.Content()
                .Column(col => {

                    col.Item().Text($"{optimizations.PartsPerMaterial.Count()} Lengths").Bold().FontSize(16);

                    int i = 1;
                    foreach (var part in optimizations.PartsPerMaterial) {

                        string val = string.Join(";  ", part.Select(p => Math.Round(p.AsMillimeters(), 1).ToString()));

                        col.Item()
                            .PaddingLeft(20)
                            .Text($"{i++} - {val}");

                    }

                    if (optimizations.UnplacedParts.Any()) {

                        col.Item()
                            .PaddingTop(30)
                            .Text("Unplaced Items")
                            .Bold()
                            .FontSize(16);

                        foreach (var part in optimizations.UnplacedParts) {
                            col.Item()
                                .PaddingLeft(20)
                                .Text(Math.Round(part.AsMillimeters()).ToString());
                        }

                    }

                });

        });

    }

}
