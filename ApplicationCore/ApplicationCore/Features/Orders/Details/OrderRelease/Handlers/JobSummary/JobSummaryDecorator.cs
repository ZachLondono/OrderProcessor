using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.JobSummary;

internal class JobSummaryDecorator : IDocumentDecorator {

    public void Decorate(Order order, IDocumentContainer container) {

        container.Page(page => {

            page.Content().Text($"Job Summary {order.Number} {order.Name}");

        });

    }

    public static string GetCabinetSeries(Cabinet cabinet) {

        if (cabinet.BoxMaterial.Core == CabinetMaterialCore.Plywood) return "Sterling 18_5";
        else if (cabinet.BoxMaterial.Core == CabinetMaterialCore.Flake && cabinet.FinishMaterial.Core == CabinetMaterialCore.Flake) return "Crown Paint";
        else if (cabinet.BoxMaterial.Core == CabinetMaterialCore.Flake && cabinet.FinishMaterial.Core == CabinetMaterialCore.Plywood) return "Crown Veneer";

        return "Unknown";

    }

    public static string GetCabientFinish(CabinetFinishMaterial material) {

        if (material.PaintColor is not null) {

            return "Paint " + material.PaintColor;

        }

        string suffix = material.Core switch {
            CabinetMaterialCore.Flake => "Mela",
            CabinetMaterialCore.Plywood => "Veneer",
            _ => ""
        };

        return material.Finish + " " + suffix;

    }

}
