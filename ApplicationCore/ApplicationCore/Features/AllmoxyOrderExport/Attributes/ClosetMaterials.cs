namespace ApplicationCore.Features.AllmoxyOrderExport.Attributes;

public static class ClosetMaterials {

    public static string GetMatchingMaterialName(string color) {
        return color;
        // throw new NotImplementedException();
    }

    public static string[] MaterialNames => new string[] {
        "White"
    };

}
