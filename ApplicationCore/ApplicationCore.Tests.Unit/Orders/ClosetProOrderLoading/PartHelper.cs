using ApplicationCore.Features.ClosetProCSVCutList;
using ApplicationCore.Features.ClosetProCSVCutList.CSVModels;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Closets;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.ClosetProOrderLoading;

public class PartHelper {

    public Part Part { get; set; } = new();

    public ClosetPart CompareToProduct(IProduct product) {

        product.Should().BeOfType<ClosetPart>();

        var closetPart = product as ClosetPart;
        closetPart.Should().NotBeNull();
        closetPart!.Material.Core.Should().Be(ClosetMaterialCore.ParticleBoard);
        closetPart.Material.Finish.Should().Be(Part.Color);
        closetPart.Qty.Should().Be(Part.Quantity);
        closetPart.ProductNumber.Should().Be(Part.PartNum);
        closetPart.Room.Should().Be(ClosetProPartMapper.GetRoomName(Part, RoomNamingStrategy.ByWallAndSection));
        if (ClosetProPartMapper.TryParseMoneyString(Part.PartCost, out decimal price)) {
            closetPart.UnitPrice.Should().Be(price);
        }

        return closetPart;

    }

}
