using ApplicationCore.Features.Orders.OrderLoading;

namespace ApplicationCore.Features.Orders.OrderLoading.PickOrderSource;

public class SourceConfig {

    public required string Name { get; set; }

    public required OrderSourceType SourceType { get; set; }

    public required string DialogTitle { get; set; }

    public required Type SourcePickerDialogType { get; set; }

}
