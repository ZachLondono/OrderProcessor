using Domain.Orders.Builders;
using Domain.Orders.Enums;
using Domain.ValueObjects;

namespace Domain.Tests.Unit.Orders.Products.CabinetDrawerboxes;

public class DBBuilderFactoryFactory {

    /*
     * This is here because the drawer box construction variables are saved in static properties which need to be set in unit tests without affecting other unit tests
     * This will probably still not work with tests running in parallel which both change static properties... the way cabinets create drawer boxes should probably be reworked 
     */

    public Func<DovetailDrawerBoxBuilder> CreateBuilderFactory(Action? configureStaticProperties = null) {

        return () => {

            ResetDBBuilder();
            configureStaticProperties?.Invoke();

            return new();

        };

    }

    public static void ResetDBBuilder() {
        DovetailDrawerBoxBuilder.VerticalClearance = Dimension.FromMillimeters(41);

        DovetailDrawerBoxBuilder.DividerThickness = Dimension.FromMillimeters(19);

        DovetailDrawerBoxBuilder.RollOutBlockThickness = Dimension.FromInches(1);

        DovetailDrawerBoxBuilder.StdHeights = new() {
                Dimension.FromMillimeters(57),
                Dimension.FromMillimeters(64),
                Dimension.FromMillimeters(86),
                Dimension.FromMillimeters(105),
                Dimension.FromMillimeters(137),
                Dimension.FromMillimeters(159),
                Dimension.FromMillimeters(181),
                Dimension.FromMillimeters(210),
                Dimension.FromMillimeters(260),
            };

        DovetailDrawerBoxBuilder.DrawerSlideWidthAdjustments = new() {
                {  DrawerSlideType.UnderMount, Dimension.FromMillimeters(10) },
                {  DrawerSlideType.SideMount, Dimension.FromMillimeters(26) }
            };

        DovetailDrawerBoxBuilder.DrawerSlideDepthClearance = new() {
                {  DrawerSlideType.UnderMount, Dimension.FromMillimeters(10) },
                {  DrawerSlideType.SideMount, Dimension.FromMillimeters(0) }
            };

        DovetailDrawerBoxBuilder.CabinetUnderMountDrawerSlideBoxDepths = new Dimension[] {
                Dimension.FromMillimeters(250),
                Dimension.FromMillimeters(280),
                Dimension.FromMillimeters(320),
                Dimension.FromMillimeters(381),
                Dimension.FromMillimeters(450),
                Dimension.FromMillimeters(550)
            };

        DovetailDrawerBoxBuilder.RollOutSetBack = Dimension.FromMillimeters(2);
    }

}
