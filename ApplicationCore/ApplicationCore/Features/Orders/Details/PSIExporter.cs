using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Features.Orders.Domain.Products;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.ProductPlanner.Services;

namespace ApplicationCore.Features.Orders.Details;

public class PSIExporter {

    public void ExportOrder(Order order, string exportDirectory) {

        var extWriter = new ExtWriter();
        var rooms = order.Products.Where(p => p is Cabinet).Cast<Cabinet>() //.GroupBy(cab => cab.Room); TODO: add rooms to products
                                                            .GroupBy(c => ""); // TODO: remove this, it is just to make it work for a test

        // TODO: number should come from the cabinet entity
        int productIndex = 0;

        int jobId = 0;
        // TODO: if there are not multiple rooms or multiple different materials, all products can be put in the same top level job without any levels
        extWriter.AddRecord(new JobDescriptor() {
            LevelId = jobId,
            Job = order.Name,
            Date = order.OrderDate,
            Catalog = "",
            Fronts = "",
            Hardware = "",
            Materials = ""
        });

        int roomIdx = 0;
        foreach (var room in rooms) {

            if (!room.Any()) continue;

            roomIdx++;

            var materialGroups = room.GroupBy(cab => (cab.BoxMaterial.Finish, cab.BoxMaterial.Core, cab.FinishMaterial.Finish, cab.FinishMaterial.Core));
            bool multipleMaterials = (materialGroups.Count() > 1);

            var firstMaterials = materialGroups.First().Key;
            var level = new LevelDescriptor() {
                LevelId = jobId + roomIdx,
                ParentId = jobId,
                Name = (string.IsNullOrEmpty(room.Key) ? $"Lvl{roomIdx}" : room.Key),
                Catalog = "Royal2",
                Materials = GetMaterial(firstMaterials.Item2, firstMaterials.Item4),
                Fronts = "Buyout",//GetPSIDoorType(firstMaterials.Item5),
                Hardware = "Standard"
            };

            if (!multipleMaterials) {
                extWriter.AddRecord(GetMaterialVariableRecord(firstMaterials.Item2, firstMaterials.Item1, firstMaterials.Item4, firstMaterials.Item3, jobId + roomIdx));
            }

            extWriter.AddRecord(level);

            int materialIdx = 0;
            foreach (var material in materialGroups) {

                materialIdx++;

                if (multipleMaterials) {
                    int lvlId = jobId + roomIdx + materialIdx;
                    extWriter.AddRecord(new LevelDescriptor() {
                        LevelId = lvlId,
                        ParentId = roomIdx,
                        Name = $"{materialIdx}-{(string.IsNullOrEmpty(room.Key) ? $"Lvl{roomIdx}" : room.Key)}",
                        Catalog = "Royal2",
                        Materials = GetMaterial(material.Key.Item2, material.Key.Item4),
                        Fronts = "Buyout", //GetPSIDoorType(material.Key.Item5),
                        Hardware = "Standard"
                    });

                    extWriter.AddRecord(GetMaterialVariableRecord(material.Key.Item2, material.Key.Item1, material.Key.Item4, material.Key.Item3, lvlId));
                }

                foreach (var cab in material) {

                    int parentId = jobId + roomIdx + (multipleMaterials ? materialIdx : 0);

                    extWriter.AddRecord(new Product() {
                        Name = "B1D",       // TODO: get product sku depending on product parameters
                        ParentId = parentId,
                        Pos = ++productIndex,
                        CustomSpec = true,
                        Qty = 1,
                        SeqText = "",
                        Units = PSIUnits.Millimeters,
                        // TODO: get parameters from product
                        Parameters = new() {
                            { "ProductW", "500" },
                            { "ProductH", "500" },
                            { "ProductD", "500" }
                        }
                    });

                }

            }

            roomIdx += materialIdx;

        }

        var filePath = Path.Combine(exportDirectory, $"{order.Number} - {order.Name}.ext");

        extWriter.WriteFile(filePath);

    }

    private static string GetMaterial(CabinetMaterialCore boxMaterial, CabinetMaterialCore finishMaterial) {

        if (boxMaterial == CabinetMaterialCore.Plywood) return "Sterling 18_5";
        else if (boxMaterial == CabinetMaterialCore.Flake && finishMaterial == CabinetMaterialCore.Flake) return "Crown Paint";
        else if (boxMaterial == CabinetMaterialCore.Flake && finishMaterial == CabinetMaterialCore.Plywood) return "Crown Veneer";

        throw new InvalidOperationException("Unexpected material combination");

    }

    /*private string GetPSIDoorType(DoorType doorType) => doorType switch {
        DoorType.Slab => "Slab",
        DoorType.MDF => "Buyout",
        _ => "Buyout"
    };*/

    private VariableOverride GetMaterialVariableRecord(CabinetMaterialCore boxMaterial, string boxColor, CabinetMaterialCore finishMaterial, string finishColor, int levelId) {

        var materials = new Dictionary<string, string> {
            {
                "F_Exp_SemiExp",
                new PSIMaterial() {
                    Material = GetFinishMaterialType(finishMaterial),
                    Color = finishColor,
                    Thickness = 0,
                    Units = PSIUnits.Millimeters
                }.ToString()
            },

            {
                "F_Exp_Unseen",
                new PSIMaterial() {
                    Material = GetFinishMaterialType(finishMaterial),
                    Color = finishColor,
                    Thickness = 0,
                    Units = PSIUnits.Millimeters
                }.ToString()
            },

            {
                "F_Exposed",
                new PSIMaterial() {
                    Material = GetFinishMaterialType(finishMaterial),
                    Color = finishColor,
                    Thickness = 0,
                    Units = PSIUnits.Millimeters
                }.ToString()
            },

            {
                "F_OvenSupport",
                new PSIMaterial() {
                    Color = "PRE",
                    Material = "Veneer",
                    Thickness = 0,
                    Units = PSIUnits.Millimeters
                }.ToString()
            },

            {
                "F_SemiExp_Unseen",
                new PSIMaterial() {
                    Material = GetFinishMaterialType(boxMaterial),
                    Color = boxColor,
                    Thickness = 0,
                    Units = PSIUnits.Millimeters
                }.ToString()
            },

            {
                "F_SemiExposed",
                new PSIMaterial() {
                    Material = GetFinishMaterialType(boxMaterial),
                    Color = boxColor,
                    Thickness = 0,
                    Units = PSIUnits.Millimeters
                }.ToString()
            },

            {
                "EB_Case",
                new PSIMaterial() {
                    Material = GetEBMaterialType(finishMaterial),
                    Color = finishColor,
                    Thickness = 0,
                    Units = PSIUnits.Millimeters
                }.ToString()
            },

            {
                "EB_Inside",
                new PSIMaterial() {
                    Material = GetEBMaterialType(boxMaterial),
                    Color = boxColor,
                    Thickness = 0,
                    Units = PSIUnits.Millimeters
                }.ToString()
            },

            {
                "EB_ShellExposed",
                new PSIMaterial() {
                    Material = GetEBMaterialType(finishMaterial),
                    Color = finishColor,
                    Thickness = 0,
                    Units = PSIUnits.Millimeters
                }.ToString()
            },

            {
                "EB_WallBottom",
                new PSIMaterial() {
                    Material = GetEBMaterialType(finishMaterial),
                    Color = finishColor,
                    Thickness = 0,
                    Units = PSIUnits.Millimeters
                }.ToString()
            }
        };

        var variables = new VariableOverride {
            Units = PSIUnits.Millimeters,
            Parameters = materials,
            LevelId = levelId
        };

        return variables;

    }

    private string GetFinishMaterialType(CabinetMaterialCore material) => material switch {
        CabinetMaterialCore.Flake => "Mela",
        CabinetMaterialCore.Plywood => "Veneer",
        _ => "Mela"
    };

    private string GetEBMaterialType(CabinetMaterialCore material) => material switch {
        CabinetMaterialCore.Flake => "PVC",
        CabinetMaterialCore.Plywood => "Veneer",
        _ => "PVC"
    };

}
