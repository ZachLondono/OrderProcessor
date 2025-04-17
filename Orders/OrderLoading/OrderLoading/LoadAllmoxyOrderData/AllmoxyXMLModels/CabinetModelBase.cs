using Domain.Orders.Builders;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.ValueObjects;
using System.Xml.Serialization;
using OneOf.Types;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public abstract class CabinetModelBase : ProductOrItemModel {

	[XmlAttribute("groupNumber")]
	public int GroupNumber { get; set; }

	[XmlAttribute("lineNumber")]
	public int LineNumber { get; set; }

	[XmlElement("cabinet")]
	public CabinetModel Cabinet { get; set; } = new();

	public int GetProductNumber() => int.Parse($"{GroupNumber}{LineNumber:00}");

	public TBuilder InitializeBuilder<TBuilder, TCabinet>(TBuilder builder) where TBuilder : CabinetBuilder<TCabinet> where TCabinet : Cabinet {

		CabinetMaterialCore boxCore = AllmoxyXMLOrderProviderHelpers.GetMaterialCore(Cabinet.BoxMaterial.Core);
		CabinetMaterialFinishType boxFinishType = AllmoxyXMLOrderProviderHelpers.GetMaterialFinishType(Cabinet.BoxMaterial.Type);
		CabinetMaterialCore finishCore = AllmoxyXMLOrderProviderHelpers.GetFinishedSideMaterialCore(Cabinet.FinishMaterial.Core, boxCore);
		CabinetMaterialFinishType finishFinishType = AllmoxyXMLOrderProviderHelpers.GetMaterialFinishType(Cabinet.FinishMaterial.Type);

		// When the finished sides are painted, the finished sides will be made out of the same material as the rest of the box and then painted over.
		// This can be done because when ordering a cabinet the finished sides can either be specified to be plywood OR they can be specified to be painted. So if the customer wants a plywood cabinet with painted sides then the box must be made of plywood as well.
		// So if the finish sides are painted, set the finished sides material's 'Finish' property to that of the box material.
		// In this case the finished sides material finish data should have a placeholder value such as 'match' to signify that it should match the box material. 
		string finishColor = (finishFinishType == CabinetMaterialFinishType.Paint ? Cabinet.BoxMaterial.Finish : Cabinet.FinishMaterial.Finish);
		string? finishPaintColor = (finishFinishType == CabinetMaterialFinishType.Paint ? Cabinet.FinishMaterial.Finish : null);
		CabinetMaterial boxMaterial = new(Cabinet.BoxMaterial.Finish, boxFinishType, boxCore);
		CabinetFinishMaterial finishMaterial = new(finishColor, finishFinishType, finishCore, finishPaintColor);
		CabinetSideType leftSideType = AllmoxyXMLOrderProviderHelpers.GetCabinetSideType(Cabinet.LeftSide);
		CabinetSideType rightSideType = AllmoxyXMLOrderProviderHelpers.GetCabinetSideType(Cabinet.RightSide);

		CabinetDoorConfiguration doorConfiguration;
		if (Cabinet.Fronts.Type == "MDF") {

			// If the door is an MDF door the finish type cannot be a melamine or veneer that 
			if (Cabinet.Fronts.FinishType == AllmoxyXMLOrderProviderHelpers.MELAMINE_FINISH_CODE || Cabinet.Fronts.FinishType == AllmoxyXMLOrderProviderHelpers.VENEER_FINISH_CODE) {
				throw new InvalidOperationException("Invalid combination of door finish and door type");
			}

			MDFDoorFinish finish = Cabinet.Fronts.FinishType switch {
				AllmoxyXMLOrderProviderHelpers.NO_FINISH_CODE => new None(),
				AllmoxyXMLOrderProviderHelpers.PAINT_FINISH_CODE => new Paint(Cabinet.Fronts.Color),
				_ => throw new InvalidOperationException($"Unknown front finish type {Cabinet.Fronts.FinishType}")
			};

			// TODO: if Cabinet.Fronts.Color == "Match Finish" then the finished sides must be painted
			doorConfiguration = new MDFDoorOptions("MDF", Dimension.FromInches(0.75), Cabinet.Fronts.Style, "Eased", "Flat", Dimension.Zero, finish);

		} else if (Cabinet.Fronts.Type == "Slab") {

			var slab = GetSlabDoorMaterial(Cabinet.Fronts, boxMaterial, finishMaterial);
			doorConfiguration = slab is not null ?  slab : new DoorsByOthers();

		} else if (Cabinet.Fronts.Type == "None") {

			doorConfiguration = new DoorsByOthers();

		} else {

			throw new InvalidDataException("Invalid cabinet door configuration.");

		}

		string edgeBandingColor;
		if (Cabinet.EdgeBandColor == "Match Finish") {

			if (Cabinet.FinishMaterial.Type == AllmoxyXMLOrderProviderHelpers.PAINT_FINISH_CODE)
				edgeBandingColor = Cabinet.BoxMaterial.Finish;
			else {
				edgeBandingColor = Cabinet.FinishMaterial.Finish;
			}

		} else {
			edgeBandingColor = Cabinet.EdgeBandColor;
		}

		return (TBuilder)builder.WithQty(Cabinet.Qty)
									.WithUnitPrice(AllmoxyXMLOrderProviderHelpers.StringToMoney(Cabinet.UnitPrice))
									.WithProductNumber(GetProductNumber())
									.WithBoxMaterial(boxMaterial)
									.WithFinishMaterial(finishMaterial)
									.WithDoorConfiguration(doorConfiguration)
									.WithLeftSideType(leftSideType)
									.WithRightSideType(rightSideType)
									.WithEdgeBandingColor(edgeBandingColor)
									.WithWidth(Dimension.FromMillimeters(Cabinet.Width))
									.WithHeight(Dimension.FromMillimeters(Cabinet.Height))
									.WithDepth(Dimension.FromMillimeters(Cabinet.Depth))
									.WithRoom(Cabinet.Room)
									.WithAssembled(Cabinet.Assembled == AllmoxyXMLOrderProviderHelpers.XML_BOOL_TRUE)
									.WithComment(Cabinet.Comment)
									.WithProductionNotes(Cabinet.ProductionNotes.Where(n => !string.IsNullOrWhiteSpace(n)).ToList());

	}

	// TODO: move to AllmoxyXMLOrderProviderHelpers class
	public static CabinetSlabDoorMaterial? GetSlabDoorMaterial(CabinetFrontsModel cabFronts, CabinetMaterial boxMaterial, CabinetFinishMaterial finishMaterial) {

		if (cabFronts.Style == "None" || cabFronts.Type != "Slab") return null;

		CabinetMaterialFinishType doorFinishType = cabFronts.FinishType == AllmoxyXMLOrderProviderHelpers.MATCH_FINISH_CODE ? finishMaterial.FinishType : AllmoxyXMLOrderProviderHelpers.GetMaterialFinishType(cabFronts.FinishType);

		// This is the default door material - to be used only when door is mdf. TODO: find a way to represent 'Buyout/MDF' for cabinet doors - maybe set this to null. Then when setting the door style, if this property is null set the style to buyout. That would allow actual buyout doors, as well as no doors, if MDFOptions is null as well as SlabDoorMaterial - an additional "ByOutDoorOptions" property would be needed at that point

		var slabMaterialFinish = (cabFronts.FinishType == AllmoxyXMLOrderProviderHelpers.MATCH_FINISH_CODE || doorFinishType == CabinetMaterialFinishType.Paint) ? finishMaterial.Finish : cabFronts.Color;
		var slabMaterialCore = AllmoxyXMLOrderProviderHelpers.GetSlabDoorMaterialCoreFromFinishType(cabFronts.FinishType, finishMaterial.Core);
		var slabDoorPaint = cabFronts.FinishType == AllmoxyXMLOrderProviderHelpers.MATCH_FINISH_CODE ? finishMaterial.PaintColor : (doorFinishType == CabinetMaterialFinishType.Paint ? cabFronts.Color : null);

		return new(slabMaterialFinish, doorFinishType, slabMaterialCore, slabDoorPaint);

	}



}
