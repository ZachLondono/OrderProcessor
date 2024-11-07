using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

public record MDFDoorOptions(string Material,
							 Dimension Thickness,
							 string FramingBead,
							 string EdgeDetail,
							 string PanelDetail,
							 Dimension PanelDrop,
							 string? PaintColor);
