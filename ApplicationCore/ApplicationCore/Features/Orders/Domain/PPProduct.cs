using ApplicationCore.Features.ProductPlanner.Contracts;

namespace ApplicationCore.Features.Orders.Domain;

public class PPProduct {

    public string Room { get; }
    public string Name { get; }
    public string Catalog { get; }
    public string MaterialType { get; }
    public string DoorType { get; }
    public string HardwareType { get; }
    public Dictionary<string, PPMaterial> FinishMaterials { get; }
    public Dictionary<string, PPMaterial> EBMaterials { get; }
    public Dictionary<string, string> Parameters { get; }
    public Dictionary<string, string> OverrideParameters { get; }

    public PPProduct(string room, string name, string catalog, string materialType, string doorType, string hardwareType, Dictionary<string, PPMaterial> finishMaterials, Dictionary<string, PPMaterial> ebMaterials, Dictionary<string, string> parameters, Dictionary<string, string> overrideParameters) {
        Room = room;
        Name = name;
        Catalog = catalog;
        MaterialType = materialType;
        DoorType = doorType;
        HardwareType = hardwareType;
        FinishMaterials = finishMaterials;
        EBMaterials = ebMaterials;
        Parameters = parameters;
        OverrideParameters = overrideParameters;
    }

}