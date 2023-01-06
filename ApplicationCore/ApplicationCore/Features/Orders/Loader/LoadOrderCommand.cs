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
		private readonly LoadingMessagePublisher _publisher;
		private readonly IMessageBoxService _messageBoxService;

        public Handler(IOrderProviderFactory factory, IBus bus, LoadingMessagePublisher publisher, IMessageBoxService messageBoxService) {
            _factory = factory;
            _bus = bus;
            _publisher = publisher;
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

            var products = new List<IProduct>();
            products.AddRange(data.DrawerBoxes.Select(MapDataToDrawerBox));

            int index = 0;
            data.BaseCabinets.ForEach(cab => {
				index++;
				try {
                    products.Add(MapDataToBaseCabinet(cab));
                } catch (Exception ex) {
					_publisher.PublishError($"Could not load cabinet {index} : {ex.Message}");
				}
            });

            data.WallCabinets.ForEach(cab => {
                index++;
                try {
                    products.Add(MapDataToWallCabinet(cab));
                } catch (Exception ex) {
                    _publisher.PublishError($"Could not load cabinet {index} : {ex.Message}");
                }
            });

            data.DrawerBaseCabinets.ForEach(cab => {
                index++;
                try {
                    products.Add(MapDataToDrawerBaseCabinet(cab));
                } catch (Exception ex) {
                    _publisher.PublishError($"Could not load cabinet {index} : {ex.Message}");
                }
            });

            data.TallCabinets.ForEach(cab => {
                index++;
                try {
                    products.Add(MapDataToTallCabinet(cab));
                } catch (Exception ex) {
                    _publisher.PublishError($"Could not load cabinet {index} : {ex.Message}");
                }
            });

            var additionalItems = data.AdditionalItems.Select(MapDataToItem);

            Response<Order> result;
            if (existingOrderId is null) {
                result = await _bus.Send(new CreateNewOrder.Command(request.Source, data.Number, data.Name, data.CustomerId, data.VendorId, data.Comment, data.OrderDate, data.Tax, data.Shipping, data.PriceAdjustment, data.Rush, data.Info, products, additionalItems));
            } else {
                result = await _bus.Send(new OverwriteExistingOrderWithId.Command((Guid)existingOrderId, request.Source, data.Number, data.Name, data.CustomerId, data.VendorId, data.Comment, data.OrderDate, data.Tax, data.Shipping, data.PriceAdjustment, data.Rush, data.Info, products, additionalItems));
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

        private static BaseCabinet MapDataToBaseCabinet(BaseCabinetData data) {

            BaseCabinetDoors doors = data.DoorQty switch {
                1 => new(data.HingeLeft ? HingeSide.Left : HingeSide.Right, data.DoorStyle),
                2 => new(data.DoorStyle),
                _ => new(data.HingeLeft ? HingeSide.Left : HingeSide.Right, data.DoorStyle)
            };
            CabinetMaterial boxMaterial = new(data.BoxMaterialFinish, data.BoxMaterialCore);
            CabinetMaterial finishMaterial = new(data.FinishMaterialFinish, data.FinishMaterialCore);
            CabinetSide leftSide = new(data.LeftSideType, data.DoorStyle);
            CabinetSide rightSide = new(data.RightSideType, data.DoorStyle);
            HorizontalDrawerBank drawers = new() {
                BoxMaterial = data.DrawerBoxMaterial,
                FaceHeight = data.DrawerFaceHeight,
                Quantity = data.DrawerQty,
                SlideType = data.DrawerBoxSlideType
            };

            BaseCabinetInside inside;
            if (data.RollOutBoxPositions.Length != 0) {
                var rollOutOptions = new RollOutOptions(data.RollOutBoxPositions, true, data.RollOutBlocks, data.DrawerBoxSlideType, data.DrawerBoxMaterial);
                inside = new(data.AdjustableShelfQty, rollOutOptions);
            } else inside = new(data.AdjustableShelfQty, data.VerticalDividerQty);

            return BaseCabinet.Create(
                data.Qty,
                data.UnitPrice,
                data.Room,
				data.Assembled,
				data.Height,
                data.Width,
                data.Depth,
                boxMaterial,
                finishMaterial,
                data.EdgeBandingColor,
                rightSide,
                leftSide,
                doors,
                data.ToeType,
                drawers,
                inside
            );

        }

        private static WallCabinet MapDataToWallCabinet(WallCabinetData data) {

            WallCabinetDoors doors = data.DoorQty switch {
                1 => new(data.HingeLeft ? HingeSide.Left : HingeSide.Right, data.DoorStyle),
                2 => new(data.DoorStyle),
                _ => new(data.HingeLeft ? HingeSide.Left : HingeSide.Right, data.DoorStyle)
            };
            CabinetMaterial boxMaterial = new(data.BoxMaterialFinish, data.BoxMaterialCore);
            CabinetMaterial finishMaterial = new(data.FinishMaterialFinish, data.FinishMaterialCore);
            CabinetSide leftSide = new(data.LeftSideType, data.DoorStyle);
            CabinetSide rightSide = new(data.RightSideType, data.DoorStyle);

            WallCabinetInside inside = new(data.AdjustableShelfQty, data.VerticalDividerQty);

            return WallCabinet.Create(
                data.Qty,
                data.UnitPrice,
                data.Room,
                data.Assembled,
                data.Height,
                data.Width,
                data.Depth,
                boxMaterial,
                finishMaterial,
                data.EdgeBandingColor,
                rightSide,
                leftSide,
                doors,
                inside);

        }

        private static DrawerBaseCabinet MapDataToDrawerBaseCabinet(DrawerBaseCabinetData data) {

            CabinetMaterial boxMaterial = new(data.BoxMaterialFinish, data.BoxMaterialCore);
            CabinetMaterial finishMaterial = new(data.FinishMaterialFinish, data.FinishMaterialCore);
            CabinetSide leftSide = new(data.LeftSideType, data.DoorStyle);
            CabinetSide rightSide = new(data.RightSideType, data.DoorStyle);

            VerticalDrawerBank verticalDrawerBank = new() {
                BoxMaterial = data.DrawerBoxMaterial,
                FaceHeights = data.DrawerFaces,
                SlideType = data.DrawerBoxSlideType
            };

            return DrawerBaseCabinet.Create(
                data.Qty,
                data.UnitPrice,
                data.Room,
                data.Assembled,
                data.Height,
                data.Width,
                data.Depth,
                boxMaterial,
                finishMaterial,
                data.EdgeBandingColor,
                rightSide,
                leftSide,
                verticalDrawerBank,
                data.DoorStyle);

        }

        private static TallCabinet MapDataToTallCabinet(TallCabinetData data) {

            CabinetMaterial boxMaterial = new(data.BoxMaterialFinish, data.BoxMaterialCore);
            CabinetMaterial finishMaterial = new(data.FinishMaterialFinish, data.FinishMaterialCore);
            CabinetSide leftSide = new(data.LeftSideType, data.DoorStyle);
            CabinetSide rightSide = new(data.RightSideType, data.DoorStyle);

            TallCabinetInside inside;
            if (data.RollOutBoxPositions.Length != 0) {
                var rollOutOptions = new RollOutOptions(data.RollOutBoxPositions, true, data.RollOutBlocks, data.DrawerBoxSlideType, data.DrawerBoxMaterial);
                inside = new(data.AdjustableShelfUpperQty, data.AdjustableShelfLowerQty, data.VerticalDividerUpperQty, rollOutOptions);
            } else inside = new(data.AdjustableShelfUpperQty, data.AdjustableShelfLowerQty, data.VerticalDividerUpperQty, data.VerticalDividerLowerQty);

            TallCabinetDoors doors;
            HingeSide hingeSide = data.LowerDoorQty == 1 ? (data.HingeLeft ? HingeSide.Left : HingeSide.Right) : HingeSide.NotApplicable;
            if (data.UpperDoorQty != 0) doors = new(data.LowerDoorHeight, hingeSide, data.DoorStyle);
            else doors = new(hingeSide, data.DoorStyle);

            return TallCabinet.Create(
                data.Qty,
                data.UnitPrice,
                data.Room,
                data.Assembled,
                data.Height,
                data.Width,
                data.Depth,
                boxMaterial,
                finishMaterial,
                data.EdgeBandingColor,
                rightSide,
                leftSide,
                doors,
                data.ToeType,
                inside);

        }

        private static AdditionalItem MapDataToItem(AdditionalItemData data) => AdditionalItem.Create(data.Description, data.Price);

    }

}
