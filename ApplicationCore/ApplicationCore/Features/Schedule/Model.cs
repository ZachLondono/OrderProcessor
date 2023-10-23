namespace ApplicationCore.Features.Schedule;

internal class Model {

    public string JobNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string JC { get; set; } = string.Empty;
    public string SC { get; set; } = string.Empty;
    public string EC { get; set; } = string.Empty;
    public string DD { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public DateTime ApprovalDate { get; set; }
    public DateTime RequestedDate { get; set; }

}
