using ApplicationCore.Features.Orders.Commands;
using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Features.Orders.Domain.Products;
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

                return new(new Error() {
					Title = "The order source is invalid",
                    Details = validation.ErrorMessage
				});

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
                    _messageBoxService.OpenDialog("Error", $"Error checking if order exists\n{error.Details}");
                }
            );

            var data = await provider.LoadOrderData(request.Source);

            if (data is null) {

                string details = "No data could read from the provided order source.";

				return new(new Error() {
					Title = "No order was read",
                    Details = details
				});

            }

            var boxes = data.Boxes.Select(d => MapDataToDrawerBox(d));
            var additionalItems = data.AdditionalItems.Select(d => MapDataToItem(d));

            Response<Order> result;
            if (existingOrderId is null) {
                result = await _bus.Send(new CreateNewOrder.Command(request.Source, data.Number, data.Name, data.CustomerId, data.VendorId, data.Comment, data.OrderDate, data.Tax, data.Shipping, data.PriceAdjustment, data.Rush, data.Info, boxes, additionalItems));
            } else {
                result = await _bus.Send(new OverwriteExistingOrderWithId.Command((Guid)existingOrderId, request.Source, data.Number, data.Name, data.CustomerId, data.VendorId, data.Comment, data.OrderDate, data.Tax, data.Shipping, data.PriceAdjustment, data.Rush, data.Info, boxes, additionalItems));
            }

            return result;
        
        }

        private static DovetailDrawerBox MapDataToDrawerBox(DrawerBoxData data) {

            var options = new DrawerBoxOptions(
                    data.BoxMaterialOptionId,
                    data.BottomMaterialOptionId,
                    data.Clips,
                    data.Notch,
                    data.Accessory,
                    data.Logo,
                    data.PostFinish,
                    data.ScoopFront,
                    data.FaceMountingHoles,
                    data.Assembled,
                    data.UBox ? new(data.UBoxA, data.UBoxB, data.UBoxC) : null,
                    data.FixedDividers ? new() { WideCount = data.DividersWide, DeepCount = data.DividersDeep } : null

                );

            return DovetailDrawerBox.Create(
                    data.Line,
                    data.UnitPrice,
                    data.Qty,
                    data.Height,
                    data.Width,
                    data.Depth,
                    data.Note,
                    data.LabelFields,
                    options
                );

        }

        private static AdditionalItem MapDataToItem(AdditionalItemData data) => AdditionalItem.Create(data.Description, data.Price);

    }

}
