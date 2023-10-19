using ApplicationCore.Features.Orders.WorkingDirectory;

namespace ApplicationCore.Widgets.Orders.WorkingDirectory;

internal class EditModel {

    public string OriginalDirectory { get; set; } = string.Empty;

    public string NewDirectory { get; set; } = string.Empty;

    public MigrationType Mode { get; set; }

}
