namespace ApplicationCore.Features.ProductPlanner.Contracts;

public class PPProduct {

    public Guid ProductId { get; }
    public string Room { get; }
    public string Name { get; }
    public int SequenceNum { get; }
    public string Catalog { get; }
    public string MaterialType { get; }
    public string DoorType { get; }
    public string HardwareType { get; }
    public string Comment { get; }
    public IDictionary<string, PPMaterial> FinishMaterials { get; }
    public IDictionary<string, PPMaterial> EBMaterials { get; }
    public IDictionary<string, string> Parameters { get; }
    public IDictionary<string, string> OverrideParameters { get; }
    public IDictionary<string, string> ManualOverrideParameters { get; }

    public PPProduct(Guid productId, string room, string name, int sequenceNum, string catalog, string materialType, string doorType, string hardwareType, string comment, IDictionary<string, PPMaterial> finishMaterials, IDictionary<string, PPMaterial> ebMaterials, IDictionary<string, string> parameters, IDictionary<string, string> overrideParameters, IDictionary<string, string> manualOverrideParameters) {
        ProductId = productId;
        Room = room;
        Name = name;
        SequenceNum = sequenceNum;
        Catalog = catalog;
        MaterialType = materialType;
        DoorType = doorType;
        Comment = comment;
        HardwareType = hardwareType;
        FinishMaterials = finishMaterials;
        EBMaterials = ebMaterials;
        Parameters = parameters;
        OverrideParameters = overrideParameters;
        ManualOverrideParameters = manualOverrideParameters;
    }

}