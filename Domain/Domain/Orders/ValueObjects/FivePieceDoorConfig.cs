using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

public class FivePieceDoorConfig {

    public Dimension FrameThickness { get; }
    public Dimension PanelThickness { get; }
    public string Material { get; }

    public FivePieceDoorConfig(Dimension frameThickness, Dimension panelThickness, string material) {
        FrameThickness = frameThickness;
        PanelThickness = panelThickness;
        Material = material;
    }

}
