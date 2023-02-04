namespace ApplicationCore.Features.ProductPlanner.Contracts;

public class PPProduct {

    public Guid ProductId { get; }
    public string Room { get; }
    public string Name { get; }
    public int SequenceNum { get;  }
    public string Catalog { get; }
    public string MaterialType { get; }
    public string DoorType { get; }
    public string HardwareType { get; }
    public Dictionary<string, PPMaterial> FinishMaterials { get; }
    public Dictionary<string, PPMaterial> EBMaterials { get; }
    public Dictionary<string, string> Parameters { get; }
    public Dictionary<string, string> OverrideParameters { get; }
    public Dictionary<string, string> ManualOverrideParameters { get; }

    public PPProduct(Guid productId, string room, string name, int sequenceNum, string catalog, string materialType, string doorType, string hardwareType, Dictionary<string, PPMaterial> finishMaterials, Dictionary<string, PPMaterial> ebMaterials, Dictionary<string, string> parameters, Dictionary<string, string> overrideParameters, Dictionary<string, string> manualOverrideParameters) {
        ProductId = productId;
        Room = room;
        Name = name;
        SequenceNum = sequenceNum;
        Catalog = catalog;
        MaterialType = materialType;
        DoorType = doorType;
        HardwareType = hardwareType;
        FinishMaterials = finishMaterials;
        EBMaterials = ebMaterials;
        Parameters = parameters;
        OverrideParameters = overrideParameters;
        ManualOverrideParameters = manualOverrideParameters;
    }

}