using ApplicationCore.Features.Orders.Commands;
using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Loader.Providers;
using ApplicationCore.Features.Orders.Loader.Providers.DTO;
using ApplicationCore.Features.Orders.Queries;
using ApplicationCore.Infrastructure;
using ApplicationCore.Shared;

namespace ApplicationCore.Features.Orders.Loader;

public class LoadOrderCommand {

    public record Command(OrderSourceType SourceType, string Source) : ICommand<Order>;

    public class Handler : CommandHandler<Command, Order> {

        private readonly IOrderProviderFactory _factory;
        private readonly IBus _bus;
        private readonly IMessageBoxService _messageBoxService;

        public Handler(IOrderProviderFactory factory, IBus bus, IMessageBoxService messageBoxService) {
            _factory = factory;
            _bus = bus;
            _messageBoxService = messageBoxService;
        }

        public override async Task<Response<Order>> Handle(Command request) {

            var provider = _factory.GetOrderProvider(request.SourceType);

            var validation = await provider.ValidateSource(request.Source);

            if (!validation.IsValid) {

                return new(new Error("The order source is invalid. " + validation.ErrorMessage));

            }

            var existsResult = await _bus.Send(new GetOrderIdWithSource.Query(request.Source));
            Guid? existingOrderId = null;
            existsResult.Match(
                existingId => {
                    if (existingId is null) return;
                    var result = _messageBoxService.OpenDialogYesNo("An order from this source already exists, do you want to overwrite the existing order?", "Order Exists");
                    if (result is YesNoResult.Yes) {
                        existingOrderId = existingId;
                    }
                },
                error => {
                    // TODO: log error
                    _messageBoxService.OpenDialog("Error", $"Error checking if order exists\n{error.Message}");
                }
            );

            var data = await provider.LoadOrderData(request.Source);

            if (data is null) {

                return new(new Error("Could not load order from source"));

            }

            var boxes = data.Boxes.Select(d => MapDataToDrawerBox(d));
            var additionalItems = data.AdditionalItems.Select(d => MapDataToItem(d));

            Response<Order> result;
            if (existingOrderId is null) {
                result = await _bus.Send(new CreateNewOrder.Command(request.Source, data.Number, data.Name, data.CustomerId, data.VendorId, data.Comment, data.OrderDate, data.Tax, data.Shipping, data.PriceAdjustment, data.Info, boxes, additionalItems));
            } else {
                result = await _bus.Send(new OverwriteExistingOrderWithId.Command((Guid)existingOrderId, request.Source, data.Number, data.Name, data.CustomerId, data.VendorId, data.Comment, data.OrderDate, data.Tax, data.Shipping, data.PriceAdjustment, data.Info, boxes, additionalItems));
            }

            return result;
        
        }

        private static DrawerBox MapDataToDrawerBox(DrawerBoxData data) {

            var options = new DrawerBoxOptions(
                    new(data.BoxMaterialOptionId, "", Dimension.FromMillimeters(0)), // TODO: get option names
                    new(data.BottomMaterialOptionId, "", Dimension.FromMillimeters(0)),
                    new(data.ClipsOptionId, ""),
                    new(data.ClipsOptionId,""),
                    new(data.NotchOptionId, ""),
                    data.Logo,
                    data.PostFinish,
                    data.ScoopFront,
                    data.FaceMountingHoles,
                    data.UBox ? new(data.UBoxA, data.UBoxB, data.UBoxC) : null,
                    data.FixedDividers ? new() : null

                );

            return DrawerBox.Create(
                    data.Line,
                    data.UnitPrice,
                    data.Qty,
                    data.Height,
                    data.Width,
                    data.Depth,
                    options
                );

        }

        private static AdditionalItem MapDataToItem(AdditionalItemData data) => AdditionalItem.Create(data.Description, data.Price);

    }

}
