using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record BaseCabinetConstructionParameters(Dimension DoorTopClearance,
                                                Dimension DoorSideClearance,
                                                Dimension DoorVerticalClearance,
                                                Dimension DoorHorizontalClearance,
                                                Dimension MaterialThickness,
                                                Func<DrawerSlideType, Dimension> GetSlideWidthAdjustment);