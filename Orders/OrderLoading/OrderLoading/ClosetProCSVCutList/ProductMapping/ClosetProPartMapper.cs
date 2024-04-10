using OrderLoading.ClosetProCSVCutList.CSVModels;
using OrderLoading.ClosetProCSVCutList.Products;
using Domain.Orders.Builders;
using Domain.Extensions;
using Domain.ValueObjects;

namespace OrderLoading.ClosetProCSVCutList;

public partial class ClosetProPartMapper(ComponentBuilderFactory factory) {

	public bool GroupLikeProducts { get; set; } = false;
	public RoomNamingStrategy RoomNamingStrategy { get; set; } = RoomNamingStrategy.ByWallAndSection;

	private readonly ComponentBuilderFactory _factory = factory;

	public List<IClosetProProduct> MapPartsToProducts(IEnumerable<Part> parts, Dimension hardwareSpread) {


		var products = parts.GroupBy(p => p.WallNum)
							.SelectMany(p => GetPartsForWall(p, hardwareSpread));

		if (GroupLikeProducts) {
			products = GroupProducts(products);
		}

		return products.ToList();

	}

	private List<IClosetProProduct> GetPartsForWall(IEnumerable<Part> parts, Dimension hardwareSpread) {

		List<IClosetProProduct> products = [];

		Dictionary<int, double> sectionDepths = parts.Where(p => p.ExportName.Contains("Vert"))
													 .DistinctBy(p => p.SectionNum)
													 .ToDictionary(p => p.SectionNum, p => p.Depth);
		bool doesWallHaveBacking = parts.Any(p => p.ExportName == "Backing");

		var enumerator = parts.GetEnumerator();
		if (!enumerator.MoveNext()) {
			throw new InvalidOperationException("No products found in order");
		}
		Part part = enumerator.Current;

		while (true) {

			Part? nextPart = null;
			if (part.PartName == "Melamine") {

				products.Add(CreateFiller(part, RoomNamingStrategy));

			} else if (part.PartType == "Countertop") {

				if (part.Height != 0.75) {
					throw new InvalidOperationException($"Unsupported counter top thickness '{part.Height}', only 3/4\" is supported");
				}

				products.Add(CreateTop(part, RoomNamingStrategy));

			} else if (part.PartName == "Cab Door Rail" || part.PartName.Contains("Drawer") && part.PartName.Contains("Rail")) {

				// TODO: Cabinet door parts have a hinge direction, if that information is to be added to the product it will need to be read

				if (!enumerator.MoveNext()) {
					throw new InvalidOperationException("Unexpected end of part list");
				}

				var insertPart = enumerator.Current;
				if (insertPart.PartName != "Cab Door Insert" && !(insertPart.PartName.Contains("Drawer") && insertPart.PartName.Contains("Insert"))) {
					throw new InvalidOperationException("Door/Drawer rail part found without door/drawer insert");
				}

				products.Add(CreateFrontFromParts(part, insertPart, hardwareSpread, RoomNamingStrategy));
				if (enumerator.MoveNext()) {
					part = enumerator.Current;
					continue;
				} else {
					break;
				}

			} else if (part.ExportName == "FixedShelf" && enumerator.MoveNext()) {

				var possibleCubbyPart = enumerator.Current;
				if (possibleCubbyPart.ExportName == "Cubby-V" || possibleCubbyPart.ExportName == "Cubby-H") {

					var cubbyProducts = CreateCubbyProducts(enumerator, part, possibleCubbyPart, doesWallHaveBacking, RoomNamingStrategy);
					products.AddRange(cubbyProducts);

					if (enumerator.MoveNext()) {
						part = enumerator.Current;
						continue;
					} else {
						break;
					}

				}

				bool extendBack = false;
				if (sectionDepths.TryGetValue(part.SectionNum, out var depth)) {
					if (part.Depth == depth && doesWallHaveBacking) {
						extendBack = true;
					}
				}

				products.Add(CreateFixedShelfFromPart(part, doesWallHaveBacking, extendBack, RoomNamingStrategy));
				nextPart = possibleCubbyPart;

			} else if (part.ExportName == "AdjustableShelf") {

				bool extendBack = false;
				if (sectionDepths.TryGetValue(part.SectionNum, out var depth)) {
					if (part.Depth == depth && doesWallHaveBacking) {
						extendBack = true;
					}
				}

				products.Add(CreateAdjustableShelfFromPart(part, doesWallHaveBacking, extendBack, RoomNamingStrategy));

			} else {

				products.Add(MapSinglePartToProduct(part, doesWallHaveBacking, hardwareSpread, RoomNamingStrategy));

			}

			if (nextPart is not null) {
				part = nextPart;
			} else if (enumerator.MoveNext()) {
				part = enumerator.Current;
			} else {
				break;
			}

		}

		return products;

	}

	private static IClosetProProduct MapSinglePartToProduct(Part part, bool wallHasBacking, Dimension hardwareSpread, RoomNamingStrategy strategy) {

		return part.ExportName switch {
			"CPS FM Vert" or "CPS WM Vert" or
			"CPS WM Vert Radius" or "CPS WM Vert Straight" or
			"VP-Corner Floor Mount" or "VP-Corner Wall Hung" or
			"Vert Bottom"
				=> CreateVerticalPanelFromPart(part, wallHasBacking, strategy),

			"VP-Hutch" => CreateHutchVerticalPanel(part, wallHasBacking, strategy),

			"FixedShelf" => CreateFixedShelfFromPart(part, wallHasBacking, false, strategy),

			"AdjustableShelf" => CreateAdjustableShelfFromPart(part, wallHasBacking, false, strategy),

			"ShoeShelf" => CreateShoeShelf(part, false, wallHasBacking, strategy),

			"Toe Kick_3.75" or "Toe Kick_2.5"
				=> CreateToeKick(part, strategy),

			"Cleat" => CreateCleat(part, strategy),

			"Melamine Sidemount" or "Melamine Undermount"
				=> CreateDowelDrawerBox(part, strategy),

			"Dovetail Sidemount" or "Dovetail Undermount" or "Scoop Front Box"
				=> CreateDovetailDrawerBox(part, strategy),

			"Zargen" => CreateZargenDrawerBox(),

			"Slab" => CreateSlabFront(part, hardwareSpread, strategy),

			"Filler Panel" => CreateFiller(part, strategy),

			"Backing" or "Back-Panel" => CreateBacking(part, strategy),

			_ => throw new InvalidOperationException($"Unexpected part [{part.PartNum}] {part.PartName} / {part.ExportName}")
		};

	}

	private static IEnumerable<IClosetProProduct> CreateCubbyProducts(IEnumerator<Part> enumerator, Part part, Part firstCubbyPart, bool doesWallHaveBacking, RoomNamingStrategy strategy) {

		var accum = new CubbyAccumulator();
		accum.AddBottomShelf(part);
		if (firstCubbyPart.ExportName == "Cubby-V") {
			accum.AddVerticalPanel(firstCubbyPart);
		} else {
			accum.AddHorizontalPanel(firstCubbyPart);
		}

		Part cubbyPart;
		bool areAllCubbyPartsRead = false;
		while (!areAllCubbyPartsRead) {

			if (!enumerator.MoveNext()) {
				throw new InvalidOperationException("Ran out of parts before all cubby parts where found");
			}

			cubbyPart = enumerator.Current;

			switch (cubbyPart.ExportName) {
				case "Cubby-V":
					accum.AddVerticalPanel(cubbyPart);
					break;
				case "Cubby-H":
					accum.AddHorizontalPanel(cubbyPart);
					break;
				case "FixedShelf":
					if ((cubbyPart.PartName == "Top Fixed Shelf" || cubbyPart.PartName == "Bottom Fixed Shelf") && doesWallHaveBacking) {
						throw new InvalidOperationException("Cannot create a top/bottom divider shelf with extended back for back panel. A closet section/wall with a back panel must have a top & bottom fixed shelf with an 'ExtendedBack' to support the back panel, divider shelves do not support this.");
					}
					accum.AddTopShelf(cubbyPart);
					areAllCubbyPartsRead = true;
					break;
				default:
					throw new InvalidOperationException("Unexpected part found within cubby parts");
			}

		}

		var cubby = accum.CreateCubby(strategy);

		var prods = new List<IClosetProProduct>() {
			cubby.TopDividerShelf,
			cubby.BottomDividerShelf,
		};

		prods.AddRange(cubby.DividerPanels);
		prods.AddRange(cubby.FixedShelves);

		return prods;


	}

	public static List<OtherPart> MapPickListToItems(IEnumerable<PickPart> parts, Dictionary<string, Dimension> frontHardwareSpreads, out Dimension hardwareSpread) {

		hardwareSpread = Dimension.Zero;

		List<Dimension> spreads = [];

		List<OtherPart> items = [];
		foreach (var item in parts) {

			if (!TryParseMoneyString(item.Cost, out var cost)) {
				cost = 0;
			}

			items.Add(new() {
				Qty = item.Quantity,
				Name = item.Name,
				UnitPrice = cost
			});

			if (item.Type == "Pull/Knob") {
				// TODO: check pick list for "Pull/Knob" part types, if there is only one then the drilling spacing for drawer fronts can be inferred from that, if there are multiple then spacing cannot be inferred 
				if (frontHardwareSpreads.TryGetValue(item.Name, out Dimension spread)) {
					spreads.Add(spread);
				}
			}

		}

		if (spreads.Count == 1) {
			hardwareSpread = spreads[0];
		} else {
			// Hardware spread is unknown
			hardwareSpread = Dimension.Zero;
		}

		return items;

	}

	public static List<OtherPart> MapAccessoriesToItems(IEnumerable<Accessory> accessories) {

		List<OtherPart> items = [];

		foreach (var accessory in accessories) {

			if (!TryParseMoneyString(accessory.Cost, out var cost)) {
				cost = 0;
			}

			items.Add(new() {
				Qty = accessory.Quantity,
				Name = accessory.Name,
				UnitPrice = (cost / (decimal)accessory.Quantity)
			});

		}

		return items;

	}

	public static List<OtherPart> MapBuyOutPartsToItems(IEnumerable<BuyOutPart> parts) {

		List<OtherPart> items = [];

		foreach (var part in parts) {

			if (!TryParseMoneyString(part.PartCost, out decimal unitPrice)) {
				unitPrice = 0M;
			}

			string name;
			if (part.PartName == "Hang Rod") {
				name = $"{part.PartName} - {part.Color} - {part.Width}\"L";
			} else {
				name = part.PartName;
			}

			items.Add(new() {
				Qty = part.Quantity,
				Name = name,
				UnitPrice = (unitPrice / (decimal)part.Quantity)
			});

		}

		return items;

	}

	public static string GetRoomName(Part part, RoomNamingStrategy strategy) => strategy switch {
		RoomNamingStrategy.ByWallAndSection => $"Wall {part.WallNum} Sec {part.SectionNum}",
		RoomNamingStrategy.ByWall => $"Wall {part.WallNum}",
		RoomNamingStrategy.None or _ => "Room 1",
	};

	public static bool TryParseMoneyString(string text, out decimal value) {
		return decimal.TryParse(text.Replace("$", ""), out value);
	}

	public static string GetDividerShelfSuffix(HorizontalDividerPanelEndDrillingType type) => type switch {
		HorizontalDividerPanelEndDrillingType.FiveMM => "5",
		HorizontalDividerPanelEndDrillingType.FiveMMSingleCams => "5C",
		HorizontalDividerPanelEndDrillingType.FiveMMDoubleCams => "5C-D",
		HorizontalDividerPanelEndDrillingType.SingleCams => "",
		HorizontalDividerPanelEndDrillingType.DoubleCams => "-D",
		_ => ""
	};

	public static string GetDividerPanelSuffix(VerticalDividerPanelEndDrillingType type) => type switch {
		VerticalDividerPanelEndDrillingType.DoubleCams => "-CAM-D",
		VerticalDividerPanelEndDrillingType.SingleCams => "-CAM",
		VerticalDividerPanelEndDrillingType.NoCams => "",
		_ => ""
	};

	public static Dimension[] ParseCornerShelfDimensions(string cornerShelfSizes) {

		var dimensions = cornerShelfSizes.Split('|')
							.Select(double.Parse)
							.Select(Dimension.FromInches)
							.ToArray();

		if (dimensions.Length != 4) {
			throw new InvalidOperationException($"Unexpected corner shelf dimensions '{cornerShelfSizes}'");
		}

		return dimensions;

	}

	private static IEnumerable<IClosetProProduct> GroupProducts(IEnumerable<IClosetProProduct> products) {

		var accum = new ClosetProGroupAccumulator();
		products.ForEach(p => accum.AddProduct((dynamic)p));
		return accum.GetGroupedProducts();

	}

}

