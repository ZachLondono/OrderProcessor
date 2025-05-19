using ClosedXML.Excel;
using Domain.Orders.Entities.Products.Doors;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using OneOf.Types;

namespace OrderLoading.LoadHafeleMDFDoorSpreadsheetOrderData.ReadOrderFile;

public class HafeleMDFDoorOrder {

    public Options Options { get; }
    public Size[] Sizes { get; }
    public Data Data { get; }

    public HafeleMDFDoorOrder(Options options, Size[] sizes, Data data) {
        Options = options;
        Sizes = sizes;
        Data = data;
    }

    public static ParsingResult<HafeleMDFDoorOrder> Load(string workbookPath) {

        using Stream stream = new FileStream(workbookPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var wb = new XLWorkbook(stream);

        List<string> errors = [];
        List<string> warnings = [];

        var result = Options.LoadFromWorkbook(wb);
        Options? options = result.Data;
        errors.AddRange(result.Errors);
        warnings.AddRange(result.Warnings);

        var sizes = Size.LoadFromWorkbook(wb);
        var data = Data.LoadFromWorkbook(wb);

        HafeleMDFDoorOrder? order = null;
        if (options is not null && sizes is not null && data is not null) {
            order = new HafeleMDFDoorOrder(options, sizes, data);
        }

        return new(warnings, errors, order);

    }

    public ICollection<MDFDoorProduct> GetProducts() {

        if (!Data.MaterialThicknessesByName.TryGetValue(Options.Material, out var materialThickness)) {
            throw new InvalidOperationException($"Material '{Options.Material}' not found in material thicknesses look up");
        }

        var thicknessDim = Dimension.FromInches(materialThickness);
        var finish = GetFinish(Options.Finish);
        decimal markUp = (decimal) Data.HafeleMarkUpToCustomers;

        List<MDFDoorProduct> doors = [];

        foreach (var size in Sizes) {
            var door = CreateProduct(size, finish, thicknessDim, markUp);
            doors.Add(door);
        }

        return doors;

    }

    private MDFDoorProduct CreateProduct(Size size, MDFDoorFinish finish, Dimension thickness, decimal markUp) {

        AdditionalOpening[] additionalOpenings;
        DoorType doorType;
        MDFDoorPanel panel = new SolidPanel();
        string framingBead = Options.DoorStyle;

        switch (size.Type) {

            case "Single Panel":
                doorType = DoorType.Door;
                additionalOpenings = [];
                break;

            case "Single Open Panel, w/ Gasket Route":
                doorType = DoorType.Door;
                additionalOpenings = [];
                panel = new OpenPanel(true, true);
                break;

            case "Single Open Panel, no rabbet":
                doorType = DoorType.Door;
                additionalOpenings = [];
                panel = new OpenPanel(false, false);
                break;

            case "Drawer Front - A":
            case "Drawer Front - B":
                doorType = DoorType.DrawerFront;
                additionalOpenings = [];
                break;

            case "Double Panel, SS":
                doorType = DoorType.Door;
                additionalOpenings = [
                        new(Dimension.FromInches(size.Rail3), Dimension.FromInches(size.Panel1Height), new SolidPanel())
                    ];
                break;

            case "Double Panel, OS":
                doorType = DoorType.Door;
                additionalOpenings = [
                        new(Dimension.FromInches(size.Rail3), Dimension.FromInches(size.Panel1Height), new OpenPanel(true, false))
                    ];
                break;

            case "Double Panel, OS, w/ Gasket Route":
                doorType = DoorType.Door;
                additionalOpenings = [
                        new(Dimension.FromInches(size.Rail3), Dimension.FromInches(size.Panel1Height), new OpenPanel(true, true))
                    ];
                break;

            case "Double Panel, SO":
                doorType = DoorType.Door;
                panel = new OpenPanel(true, false);
                additionalOpenings = [
                        new(Dimension.FromInches(size.Rail3), Dimension.FromInches(size.Panel1Height), new SolidPanel())
                    ];
                break;

            case "Double Panel, OO":
                doorType = DoorType.Door;
                panel = new OpenPanel(true, false);
                additionalOpenings = [
                        new(Dimension.FromInches(size.Rail3), Dimension.FromInches(size.Panel1Height), new OpenPanel(true, false))
                    ];
                break;

            case "Double Panel, OO, w/ Gasket Route":
                doorType = DoorType.Door;
                panel = new OpenPanel(true, false);
                additionalOpenings = [
                        new(Dimension.FromInches(size.Rail3), Dimension.FromInches(size.Panel1Height), new OpenPanel(true, true))
                    ];
                break;

            case "Triple Panel":
                doorType = DoorType.Door;
                additionalOpenings = [
                        new(Dimension.FromInches(size.Rail3), Dimension.FromInches(size.Panel1Height), new SolidPanel()),
                        new(Dimension.FromInches(size.Rail4), Dimension.FromInches(size.Panel2Height), new SolidPanel())
                    ];
                break;

            case "Quadruple Panel":
                doorType = DoorType.Door;
                additionalOpenings = [
                        new(Dimension.FromInches(size.Rail3), Dimension.FromInches(size.Panel1Height), new SolidPanel()),
                        new(Dimension.FromInches(size.Rail4), Dimension.FromInches(size.Panel2Height), new SolidPanel()),
                        new(Dimension.FromInches(size.Rail5), Dimension.FromInches(size.Panel3Height), new SolidPanel())
                    ];
                break;

            case "Slab":
                doorType = DoorType.Door;
                additionalOpenings = [];
                framingBead = "Slab";
                break;

            default:
                throw new NotImplementedException($"{size.Type} doors not implemented");

        };

        var frameSize = new DoorFrame() {
            TopRail = Dimension.FromInches(size.TopRail),
            BottomRail = Dimension.FromInches(size.BottomRail),
            LeftStile = Dimension.FromInches(size.LeftStile),
            RightStile = Dimension.FromInches(size.RightStile),
        };

        var adjustedUnitPrice = size.UnitPrice / (1 + markUp);

        return MDFDoorProduct.Create(adjustedUnitPrice,
                                    "",
                                    size.Qty,
                                    size.LineNumber,
                                    doorType,
                                    Dimension.FromInches(size.Height),
                                    Dimension.FromInches(size.Width),
                                    size.SpecialInstructions,
                                    frameSize,
                                    Options.Material,
                                    thickness,
                                    framingBead,
                                    Options.EdgeProfile,
                                    Options.PanelDetail,
                                    Dimension.FromInches(Options.PanelDrop),
                                    DoorOrientation.Vertical,
                                    additionalOpenings,
                                    finish,
                                    panel);

    }

    private static MDFDoorFinish GetFinish(string finish) => finish switch {
        "None" => new None(),
        "White Prime Only" => new Primer("White Primer"),
        "Grey Prime Only" => new Primer("Grey Primer"),
        "Black Prime Only" => new Primer("Black Primer"),
        "Standard Color" => new Paint("Standard Color"),
        "Custom Color" => new Paint("Custom Color"),
        _ => throw new InvalidOperationException($"Unexpected finish option - '{finish}'")
    };

}
