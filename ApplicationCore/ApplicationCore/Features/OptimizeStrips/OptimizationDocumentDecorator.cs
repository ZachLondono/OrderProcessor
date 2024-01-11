using ApplicationCore.Shared;
using ApplicationCore.Shared.Domain;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApplicationCore.Features.OptimizeStrips;

public class OptimizationDocumentDecorator : IDocumentDecorator {

    public required Dimension PartWidth { get; set; }
    public required Dimension MaterialLength { get; set; }
    public required string Material { get; set; }
    public required Dimension[] Lengths { get; set; }

    public void Decorate(IDocumentContainer container) {

        var optimizations = Optimizer.Optimize(MaterialLength, Lengths);

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
