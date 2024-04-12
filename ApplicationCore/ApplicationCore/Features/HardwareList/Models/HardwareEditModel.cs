namespace ApplicationCore.Features.HardwareList.Models;

public class HardwareEditModel {

    public required List<SupplyEditModel> Supplies { get; set; } = [];
    public required List<DrawerSlideEditModel> DrawerSlides { get; set; } = [];
    public required List<HangingRailEditModel> HangingRails { get; set; } = [];

}
