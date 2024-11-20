using Domain.Companies.ValueObjects;
using Domain.Orders.Builders;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using OrderLoading.ClosetProCSVCutList.CSVModels;
using OrderLoading.ClosetProCSVCutList.Products;
using OrderLoading.ClosetProCSVCutList.Products.Fronts;
using OrderLoading.ClosetProCSVCutList.Products.Shelves;
using OrderLoading.ClosetProCSVCutList.Products.Verticals;

namespace OrderLoading.ClosetProCSVCutList.PartList;

public class PartListProcessor {

	private readonly ClosetProPartMapper _partMapper;
	private readonly ComponentBuilderFactory _componentBuilderFactory;

    public PartListProcessor(ClosetProPartMapper partMapper, ComponentBuilderFactory componentBuilderFactory) {

        _partMapper = partMapper;
        _partMapper.GroupLikeProducts = true; // TODO: get this from a configuration file

        _componentBuilderFactory = componentBuilderFactory;

    }

    public PartListComponents ParsePartList(ClosetProSettings settings, IEnumerable<Part> partList, Dimension hardwareSpread) {

        var productParts = GetProductPartsFromPartList(partList);

        var cpProducts = _partMapper.MapPartsToProducts(productParts, hardwareSpread);

        var products = cpProducts.Select(p => CreateProductFromClosetProProduct(p, settings, _componentBuilderFactory))
                                 .ToList();

        (var hangingRails, var hangingRailSupplies) = GetHangingRods(partList);

        (DrawerSlide[] slides, Supply[] slideSupplies) = GetDrawerSlidesFromProducts(products);

        var supplies = GetSupplies(cpProducts);
        supplies.AddRange(hangingRailSupplies);
        supplies.AddRange(slideSupplies);

        return new(products.ToArray(),
                    supplies.ToArray(),
                    hangingRails,
                    slides);

    } 

	private static List<Supply> GetSupplies(IEnumerable<IClosetProProduct> products) {

        List<Supply> supplies = [];

        // TODO: need to check if divider panels need cams

        var shelves = products.OfType<Shelf>().ToArray();
        var corners = products.OfType<CornerShelf>().ToArray();

        // TODO: need to check if the adjustable shelves have pins or not
        int adjPins = shelves.Where(s => s.Type == ShelfType.Adjustable).Sum(s => s.Qty * 4);
        adjPins += shelves.Where(s => s.Type == ShelfType.Shoe).Sum(s => s.Qty * 4);
        adjPins += corners.Where(s => s.Type == CornerShelfType.LAdjustable || s.Type == CornerShelfType.DiagonalAdjustable).Sum(s => s.Qty * 6);
        // Closet spreadsheet adds an additional 4%
        if (adjPins > 0) {
            supplies.Add(Supply.LockingShelfPeg((int)(adjPins * 1.04)));
        }

        int cams = shelves.Where(s => s.Type == ShelfType.Fixed).Sum(s => s.Qty * 4);
        cams += corners.Where(s => s.Type == CornerShelfType.LFixed || s.Type == CornerShelfType.DiagonalFixed).Sum(s => s.Qty * 6);
        // TODO: check that toe kicks are fixed
        cams += products.OfType<MiscellaneousClosetPart>().Where(p => p.Type == MiscellaneousType.ToeKick).Sum(t => t.Qty * 4);
        cams += 8; // The closet spreadsheet add 8 extra cams
        if (cams > 0) {
            supplies.Add(Supply.RafixCam(cams));
        }

        var verticals = products.OfType<VerticalPanel>().ToArray();
        var drilledThrough = verticals.Where(v => v.Drilling == VerticalPanelDrilling.DrilledThrough).Sum(v => v.Qty);
        var finishedSide = verticals.Where(v => v.Drilling != VerticalPanelDrilling.DrilledThrough).Sum(v => v.Qty);
        // Closet spreadsheet adds an additional 5%
        if (finishedSide > 0) {
            supplies.Add(Supply.CamBolt((int)(finishedSide * 6 * 1.05)));
        }
        if (drilledThrough > 0) {
            supplies.Add(Supply.CamBoltDoubleSided((int)(drilledThrough * 6 * 1.05)));
        }

        supplies.AddRange(GetHangingRailSupplies(products));

        return supplies;

    }

    private static IEnumerable<Supply> GetHangingRailSupplies(IEnumerable<IClosetProProduct> products) {

        var verticals = products.OfType<VerticalPanel>().ToArray();
        var wallHung = verticals.Where(v => v.WallHung).ToArray();

		if (wallHung.Length == 0) return [];

		var rooms = products.GroupBy(p => p.Room)
							.Where(g => g.OfType<VerticalPanel>().Any(v => v.WallHung))
							.ToArray();
		double shelfLengths = rooms.Select(r => r.OfType<Shelf>().FirstOrDefault()?.Width ?? Dimension.Zero).Sum(d => d.AsInches());
		double panelThickness = (rooms.Length + 1) * 0.75;
		double totalLength = (shelfLengths + panelThickness) / 12.0;

        var finLeft = wallHung.Where(v => v.Drilling == VerticalPanelDrilling.FinishedLeft).Sum(v => v.Qty);
        var finRight = wallHung.Where(v => v.Drilling == VerticalPanelDrilling.FinishedRight).Sum(v => v.Qty);
        var drilledThrough = wallHung.Where(v => v.Drilling == VerticalPanelDrilling.DrilledThrough).Sum(v => v.Qty);

        return [
            .. Supply.HangingBracketLH(finLeft + (drilledThrough / 2)),
            .. Supply.HangingBracketRH(finRight + (drilledThrough / 2)),
            .. Supply.HangingRail((int) totalLength),
            Supply.LongEuroScrews((finLeft + finRight + drilledThrough) * 2),
        ];

    }

	public static (HangingRail[], Supply[]) GetHangingRods(IEnumerable<Part> parts) {

		List<HangingRail> rails = [];

		foreach (var part in parts) {

            if (part.PartName != "Hang Rod") {
                continue;
            }

			double adjLength = part.Width - 0.25;
            Dimension length = Dimension.FromMillimeters(Math.Round(Dimension.FromInches(adjLength).AsMillimeters()));
            rails.Add(new HangingRail(Guid.NewGuid(), part.Quantity, length, part.Color));

        }

		var groupedRails = rails.GroupBy(r => (r.Length, r.Finish))
					.Select(g => new HangingRail(Guid.NewGuid(), g.Sum(r => r.Qty), g.Key.Length, g.Key.Finish))
					.ToList();

		Supply[] supplies = [];
        if (rails.Count != 0) {

			var totalQty = rails.Sum(r => r.Qty);

			supplies = [
				Supply.RodMountingBracketOpen(totalQty),
				Supply.RodMountingBracketClosed(totalQty),
			];

		}

        return (groupedRails.ToArray(), supplies);

	}

    private static IProduct CreateProductFromClosetProProduct(IClosetProProduct product, ClosetProSettings settings, ComponentBuilderFactory factory) {

		if (product is CornerShelf cornerShelf) {

			return cornerShelf.ToProduct(settings);

		} else if (product is DrawerBox db) {

			return db.ToProduct(factory, settings);

		} else if (product is FivePieceFront fivePieceFront) {

			return fivePieceFront.ToProduct();

		} else if (product is HutchVerticalPanel hutch) {

			return hutch.ToProduct(settings.VerticalPanelBottomRadius);

		} else if (product is IslandVerticalPanel island) {

			return island.ToProduct();

		} else if (product is MDFFront mdfFront) {

			return mdfFront.ToProduct();

		} else if (product is MelamineSlabFront melaSlab) {

			return melaSlab.ToProduct();

		} else if (product is MiscellaneousClosetPart misc) {

			return misc.ToProduct(settings);

		} else if (product is Shelf shelf) {

			return shelf.ToProduct(settings);

		} else if (product is TransitionVerticalPanel transition) {

			return transition.ToProduct(settings.VerticalPanelBottomRadius);

		} else if (product is VerticalPanel vertical) {

			return vertical.ToProduct(settings.VerticalPanelBottomRadius);

		} else if (product is ZargenDrawerBox zargen) {

			return zargen.ToProduct();

		} else if (product is DividerShelf dividerShelf) {

			return dividerShelf.ToProduct();

		} else if (product is DividerVerticalPanel dividerPanel) {

			return dividerPanel.ToProduct();

		} else {

			throw new InvalidOperationException("Unexpected product");

		}

	}

    private static IEnumerable<Part> GetProductPartsFromPartList(IEnumerable<Part> parts) {

        string[] productPartTypes = [
            "Material",
            "Drawer",
            "Door",
            "Hamper",
            "Box",
            "Base Molding",
            "Crown Molding"
        ];

        return parts.Where(p => productPartTypes.Contains(p.PartType));

    }

	private static (DrawerSlide[], Supply[]) GetDrawerSlidesFromProducts(IEnumerable<IProduct> products) {

		var slides = products.OfType<IDrawerSlideContainer>()
								.SelectMany(d => d.GetDrawerSlides())
								.GroupBy(d => (d.Length, d.Style))
								.Select(g => new DrawerSlide(Guid.NewGuid(), g.Sum(g => g.Qty), g.Key.Length, g.Key.Style))
								.ToArray();

		Supply[] screws = [];
		if (slides.Length != 0) {

			int totalSlides = slides.Sum(s => s.Qty);

			screws = [
				Supply.DrawerSlideEuroScrews(slides.Length * 2 * totalSlides),
				Supply.ClosetDrawerClips(slides.Length * totalSlides)
			];

		}

		return (slides, screws);
	}


}