using Domain.Orders.Enums;
using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

public record BaseCabinetConstructionParameters(Dimension DoorTopClearance,
                                                Dimension DoorSideClearance,
                                                Dimension DoorVerticalClearance,
                                                Dimension DoorHorizontalClearance,
                                                Dimension MaterialThickness,
                                                Func<DrawerSlideType, Dimension> GetSlideWidthAdjustment);