using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.AllmoxyOrderExport.Attributes;

public static class DrawerBoxMaterial {

    public const string SOLID_BIRCH = "Pre-Finished Birch";
    public const string ECONOMY_BIRCH = "Economy Birch";

    public static string GetStandardHeight(Dimension faceHeight) {

        var standardHeight = DovetailDrawerBoxBuilder.GetDrawerBoxHeightFromDrawerFaceHeight(faceHeight);

        return Heights[standardHeight.AsMillimeters()];

    }

    public static Dictionary<double, string> Heights => new() {
        //57
        { 64, "2.5" },
        { 86, "3.375" },
        { 105, "4.125" },
        { 137, "5.375" },
        { 181, "7.125" },
        { 210, "8.25" },
        { 260, "10.25" },

        //2.5
        //3.375
        //4.125
        //5.375
        //6
        //6.25
        //7.125
        //8.25
        //9.25
        //10.25
        //12
        //16
    };

}
