using ApplicationCore.Features.Orders.WorkingDirectory;

namespace ApplicationCore.Features.Orders.Details.WorkingDirectory;

internal class EditModel {

    public string NewDirectory { get; set; } = string.Empty;

    public MigrationType Mode { get; set; }

}
