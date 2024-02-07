using OrderLoading.ClosetProCSVCutList;

namespace ApplicationCore.Features.ClosetProToAllmoxyOrder.Models;

public class ClosetProLoadingSettings {

    public bool GroupLikeProducts { get; set; } = true;
    public RoomNamingStrategy RoomNamingStrategy { get; set; } = RoomNamingStrategy.ByWallAndSection;

}
