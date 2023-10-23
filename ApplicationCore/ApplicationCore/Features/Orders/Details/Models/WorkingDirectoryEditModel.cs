using ApplicationCore.Features.Orders.WorkingDirectory;

namespace ApplicationCore.Features.Orders.Details.Models;

internal class WorkingDirectoryEditModel {

    public string OriginalDirectory { get; set; } = string.Empty;

    public string NewDirectory { get; set; } = string.Empty;

    public MigrationType Mode { get; set; }

}
