using ApplicationCore.Features.Orders.Shared.Domain.Enums;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

/// <summary>
/// Represents a material used in the construction of a cabinet
/// </summary>
/// <param name="Finish">The name or color of the material's finish. This may be a generic color like 'White', a vendor-specific finish like 'Tafisa Urbania White Chocolate' or a paint color</param>
/// <param name="FinishType">Describes the type of finish which is applied to the substrate. For example, 'mela' for melamine paper, 'veneer' for wood veneer or 'laminate' for laminate.</param>
/// <param name="Core">The type of wood used as the substrate for the material. For example 'flake' for flakeboard and 'ply' for plywood.</param>
public record CabinetMaterial(string Finish, CabinetMaterialFinishType FinishType, CabinetMaterialCore Core);
