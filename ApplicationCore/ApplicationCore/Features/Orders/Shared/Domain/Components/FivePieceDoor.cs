using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;
using CADCodeProxy.Machining;

namespace ApplicationCore.Features.Orders.Shared.Domain.Components;

internal class FivePieceDoor : FivePieceDoorConfig {

    public Dimension Width { get; init; }
    public Dimension Height { get; init; }
    public DoorFrame FrameSize { get; init; }

    public FivePieceDoor(Dimension width, Dimension height, DoorFrame frameSize, Dimension frameThickness, Dimension panelThickness, string material)
            : base(frameThickness, panelThickness, material) {
        Width = width;
        Height = height;
        FrameSize = frameSize;
    }

    public IEnumerable<Part> GetCNCParts(int qty, int productNumber, string customerName, string room) {
        (var top, var bottom) = GetRailParts(qty, productNumber, customerName, room);
        (var left, var right) = GetStileParts(qty, productNumber, customerName, room);
        var center = GetCenterPanelPart(qty, productNumber, customerName, room);
        List<Part> parts = new() {
            top,
            bottom,
            left,
            right,
            center
        };
        return parts;
    }

    public (Part top, Part bottom) GetRailParts(int qty, int productNumber, string customerName, string room) {

        Dimension topWidth = FrameSize.TopRail;
        Dimension bottomWidth = FrameSize.BottomRail;
        Dimension length = Width - FrameSize.LeftStile - FrameSize.RightStile;

        var top = new Part() {
            Width = topWidth.AsMillimeters(),
            Length = length.AsMillimeters(),
            Thickness = FrameThickness.AsMillimeters(),
            Qty = qty,
            IsGrained = false,
            Material = Material,
            InfoFields = new() {
                { "ProductName", "TopRail" },
                { "Description", "Five Piece Door Top Rail" },
                { "Level1", room },
                { "Comment1", "" },
                { "Comment2", "" },
                { "Side1Color", "" },
                { "Side1Material", Material },
                { "CabinetNumber", productNumber.ToString() },
                { "CustomerInfo1", customerName },
            },
            PrimaryFace = new() {
                ProgramName = $"TopRail{productNumber}",
                Tokens = Array.Empty<IToken>()
            }
        };

        var bottom = new Part() {
            Width = bottomWidth.AsMillimeters(),
            Length = length.AsMillimeters(),
            Thickness = 19.05,
            Qty = qty,
            IsGrained = false,
            Material = Material,
            InfoFields = new() {
                { "ProductName", "BottomRail" },
                { "Description", "Five Piece Door Bottom Rail" },
                { "Level1", room },
                { "Comment1", "" },
                { "Comment2", "" },
                { "Side1Color", "" },
                { "Side1Material", Material },
                { "CabinetNumber", productNumber.ToString() },
                { "CustomerInfo1", customerName },
            },
            PrimaryFace = new() {
                ProgramName = $"BottomRail{productNumber}",
                Tokens = Array.Empty<IToken>()
            }
        };

        return (top, bottom);

    }

    public (Part left, Part right) GetStileParts(int qty, int productNumber, string customerName, string room) {

        Dimension leftWidth = FrameSize.LeftStile;
        Dimension rightWidth = FrameSize.RightStile;
        Dimension length = Height - FrameSize.TopRail - FrameSize.BottomRail;

        var left = new Part() {
            Width = leftWidth.AsMillimeters(),
            Length = length.AsMillimeters(),
            Thickness = FrameThickness.AsMillimeters(),
            Qty = qty,
            IsGrained = false,
            Material = Material,
            InfoFields = new() {
                { "ProductName", "LeftStile" },
                { "Description", "Five Piece Door Left Stile" },
                { "Level1", room },
                { "Comment1", "" },
                { "Comment2", "" },
                { "Side1Color", "" },
                { "Side1Material", Material },
                { "CabinetNumber", productNumber.ToString() },
                { "CustomerInfo1", customerName },
            },
            PrimaryFace = new() {
                ProgramName = $"LeftStile{productNumber}",
                Tokens = Array.Empty<IToken>()
            }
        };

        var right = new Part() {
            Width = rightWidth.AsMillimeters(),
            Length = length.AsMillimeters(),
            Thickness = FrameThickness.AsMillimeters(),
            Qty = qty,
            IsGrained = false,
            Material = Material,
            InfoFields = new() {
                { "ProductName", "RightStile" },
                { "Description", "Five Piece Door Right Stile" },
                { "Level1", room },
                { "Comment1", "" },
                { "Comment2", "" },
                { "Side1Color", "" },
                { "Side1Material", Material },
                { "CabinetNumber", productNumber.ToString() },
                { "CustomerInfo1", customerName },
            },
            PrimaryFace = new() {
                ProgramName = $"RightStile{productNumber}",
                Tokens = Array.Empty<IToken>()
            }
        };

        return (left, right);

    }

    public Part GetCenterPanelPart(int qty, int productNumber, string customerName, string room) {

        Dimension width = Width - FrameSize.LeftStile - FrameSize.RightStile;
        Dimension length = Height - FrameSize.TopRail - FrameSize.BottomRail;

        return new() {
            Width = width.AsMillimeters(),
            Length = length.AsMillimeters(),
            Thickness = PanelThickness.AsMillimeters(),
            Qty = qty,
            IsGrained = false,
            Material = Material,
            InfoFields = new() {
                { "ProductName", "CenterPanel" },
                { "Description", "Five Piece Door Center Panel" },
                { "Level1", room },
                { "Comment1", "" },
                { "Comment2", "" },
                { "Side1Color", "" },
                { "Side1Material", Material },
                { "CabinetNumber", productNumber.ToString() },
                { "CustomerInfo1", customerName },
            },
            PrimaryFace = new() {
                ProgramName = $"CenterPanel{productNumber}",
                Tokens = Array.Empty<IToken>()
            }

        };

    }

}
