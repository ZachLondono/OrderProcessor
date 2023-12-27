using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.AllmoxyOrderExport.Attributes;

public static class DrawerBoxMaterial {

    public const string SOLID_BIRCH = "Pre-Finished Birch";
    public const string ECONOMY_BIRCH = "Economy Birch";

    public static Dimension GetStandardHeight(Dimension nonStdHeight) {
        throw new NotImplementedException();
    }

    public static Dimension[] Heights = [
        Dimension.FromInches(4.125)
    ];

}
