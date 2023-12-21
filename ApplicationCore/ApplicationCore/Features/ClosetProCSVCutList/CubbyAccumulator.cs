using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.ClosetProCSVCutList;

public class CubbyAccumulator {

    private readonly List<Part> _verticalPanels = new();
    private readonly List<Part> _horizontalPanels = new();
    private Part? _topShelf = null;
    private Part? _bottomShelf = null;

    public void AddVerticalPanel(Part part) {
        _verticalPanels.Add(part);
    }

    public void AddHorizontalPanel(Part part) {
        _horizontalPanels.Add(part);
    }

    public void AddTopShelf(Part part) {
        _topShelf = part;
    }

    public void AddBottomShelf(Part part) {
        _bottomShelf = part;
    }

    public Cubby CreateCubby() {

        if (!_verticalPanels.Any()) {
            throw new InvalidOperationException("Missing vertical cubby dividers");
        }

        if (_topShelf is null) {
            throw new InvalidOperationException("Missing cubby top shelf");
        }

        if (_bottomShelf is null) {
            throw new InvalidOperationException("Missing cubby bottom shelf");
        }

        if (_topShelf.Depth != _bottomShelf.Depth
            || _horizontalPanels.Any(p => p.Depth != _topShelf.Depth)
            || _verticalPanels.Any(p => p.Depth != _topShelf.Depth)) {
            throw new InvalidOperationException("Panels in cubby do not have matching depths");
        }

        if (_topShelf.Width != _bottomShelf.Width
            || _horizontalPanels.Any(p => p.Width != _topShelf.Width)) { 
            throw new InvalidOperationException("Horizontal panels in cubby do not have matching widths");
        }

        if (_topShelf.Color != _bottomShelf.Color
            || _verticalPanels.Any(p => p.Color != _topShelf.Color)
            || _horizontalPanels.Any(p => p.Color != _topShelf.Color)) {
            throw new InvalidOperationException("Cubby materials do not match");
        }

        if (_topShelf.WallNum != _bottomShelf.WallNum
            || _verticalPanels.Any(p => p.WallNum != _topShelf.WallNum)
            || _horizontalPanels.Any(p => p.WallNum != _topShelf.WallNum)) {
            throw new InvalidOperationException("Cubby parts are not part of the same wall");
        }

        if (_topShelf.SectionNum != _bottomShelf.SectionNum
            || _verticalPanels.Any(p => p.SectionNum != _topShelf.SectionNum)
            || _horizontalPanels.Any(p => p.SectionNum != _topShelf.SectionNum)) {
            throw new InvalidOperationException("Cubby parts are not part of the same section");
        }

        int dividerCount = _verticalPanels.Count;

        if (!ClosetProPartMapper.TryParseMoneyString(_topShelf.PartCost, out decimal topShelfPrice)) {
            topShelfPrice = 0M;
        }

        Cubby.DividerShelf topShelf = new(_topShelf.Quantity,
                                          Dimension.FromInches(_topShelf.Width),
                                          Dimension.FromInches(_topShelf.Depth),
                                          dividerCount,
                                          topShelfPrice,
                                          _topShelf.PartNum);

        if (!ClosetProPartMapper.TryParseMoneyString(_bottomShelf.PartCost, out decimal bottomShelfPrice)) {
            bottomShelfPrice = 0M;
        }

        Cubby.DividerShelf bottomShelf = new(_topShelf.Quantity,
                                          Dimension.FromInches(_bottomShelf.Width),
                                          Dimension.FromInches(_bottomShelf.Depth),
                                          dividerCount,
                                          bottomShelfPrice,
                                          _topShelf.PartNum);

        var dividerPanels = _verticalPanels.Select(p => {

            if (!ClosetProPartMapper.TryParseMoneyString(p.PartCost, out decimal unitPrice)) {
                unitPrice = 0M;
            }

            return new Cubby.DividerPanel(p.Quantity, Dimension.FromInches(p.Height), Dimension.FromInches(p.Depth), unitPrice, p.PartNum);

        }).ToArray();


        Dimension shelfWidth;
        if (_verticalPanels.Any()) {
            shelfWidth = (Dimension.FromInches(_topShelf.Width) - (Dimension.FromInches(0.75) * _verticalPanels.Count)) / (_verticalPanels.Count + 1);
        } else {
            shelfWidth = Dimension.FromInches(_topShelf.Width);
        }

        var fixedShelves = _horizontalPanels.Select(p => {

            if (!ClosetProPartMapper.TryParseMoneyString(p.PartCost, out decimal unitPrice)) {
                unitPrice = 0M;
            }

            int qty = _verticalPanels.Count + 1;
            decimal adjUnitPrice = unitPrice / qty;

            return new Cubby.FixedShelf(qty, shelfWidth, Dimension.FromInches(p.Depth), adjUnitPrice, p.PartNum);

        }).ToArray();

        var material = new ClosetMaterial(_topShelf.Color, ClosetMaterialCore.ParticleBoard);

        string edgeBandingColor = _topShelf.InfoRecords
                                            .Where(i => i.PartName == "Edge Banding")
                                            .Select(i => i.Color)
                                            .FirstOrDefault() ?? _topShelf.Color;

        string roomName = ClosetProPartMapper.GetRoomName(_topShelf);

        return new Cubby() {
            TopDividerShelf = topShelf,
            BottomDividerShelf = bottomShelf,
            DividerPanels = dividerPanels,
            FixedShelves = fixedShelves,
            Material = material,
            EdgeBandingColor = edgeBandingColor,
            Room = roomName,
        };

    }

}