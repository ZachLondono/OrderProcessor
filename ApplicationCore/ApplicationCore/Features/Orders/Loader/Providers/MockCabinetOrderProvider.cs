using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Loader.Providers.DTO;
using ApplicationCore.Features.Orders.Loader.Providers.Results;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Loader.Providers;
internal class MockCabinetOrderProvider : OrderProvider {

    public override Task<OrderData?> LoadOrderData(string source) {

        var cabinet = new BaseCabinetData() {
            Qty = 1,
            UnitPrice = 0,
            Height = Dimension.FromMillimeters(876),
            Width = Dimension.FromMillimeters(762),
            Depth = Dimension.FromMillimeters(600),
            BoxMaterialFinish = "WHITE",
            BoxMaterialCore = CabinetMaterialCore.Flake,
            FinishMaterialFinish = "WHITE",
            FinishMaterialCore = CabinetMaterialCore.Flake,
            LeftSideType = CabinetSideType.Unfinished,
            RightSideType = CabinetSideType.Unfinished,
            DoorQty = 2,
            ToeType = new LegLevelers(Dimension.FromMillimeters(102)),
            DrawerQty = 0,
            DrawerFaceHeight = Dimension.Zero,
            DrawerBoxMaterial = CabinetDrawerBoxMaterial.FingerJointBirch,
            DrawerBoxSlideType = DrawerSlideType.UnderMount,
            VerticalDividerQty = 0,
            AdjustableShelfQty = 1,
            RollOutBoxPositions = Array.Empty<Dimension>(),
            RollOutBlocks = RollOutBlockPosition.None,
            ScoopFrontRollOuts = true
        };

        return Task.FromResult<OrderData?>(new OrderData() {

            Number = "0000",
            Name = "Example Cabinet Order",
            Comment = "",
            Tax = 0,
            Shipping = 0,
            PriceAdjustment = 0,
            OrderDate = DateTime.Now,
            CustomerId = Guid.NewGuid(),
            VendorId = Guid.NewGuid(),
            Rush = false,
            BaseCabinets = new() {
                cabinet
            }
        });

    }

    public override Task<ValidationResult> ValidateSource(string source) {

        return Task.FromResult(new ValidationResult() {
            IsValid = true
        });

    }

}
