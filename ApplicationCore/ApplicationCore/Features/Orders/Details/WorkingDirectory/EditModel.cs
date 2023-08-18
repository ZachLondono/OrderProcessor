namespace ApplicationCore.Features.Orders.Details.WorkingDirectory;

internal class EditModel {

    public string NewDirectory { get; set; } = string.Empty;

    public bool CopyExistingFiles { get; set; }

    public bool DeleteExistingFiles { get; set; }

}
