using Domain.Orders.Enums;

namespace Domain.Orders.ValueObjects;

public record CabinetSlabDoorMaterial(string Finish,
									  CabinetMaterialFinishType FinishType,
									  CabinetMaterialCore Core,
									  string? PaintColor = null);
