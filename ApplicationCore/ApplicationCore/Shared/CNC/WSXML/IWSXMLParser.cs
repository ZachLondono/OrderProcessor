using ApplicationCore.Shared.CNC.WSXML.Report;

namespace ApplicationCore.Shared.CNC.WSXML;
public interface IWSXMLParser {
    CNC.Job.ReleasedJob MapDataToReleasedJob(WSXMLReport report, DateTime orderDate, DateTime? dueDate, string customerName, string vendorName);
}