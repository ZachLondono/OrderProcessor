namespace ApplicationCore.Features.Orders.Loader.Providers.Dialog;

internal class SourceConfig {

    public required string Name { get; set; }

    public required OrderSourceType SourceType { get; set; }
    
    public required string DialogTitle { get; set; }

    public required Type SourcePickerDialogType { get; set; }

}
