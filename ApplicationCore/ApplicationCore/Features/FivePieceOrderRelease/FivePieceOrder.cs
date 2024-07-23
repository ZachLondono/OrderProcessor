namespace ApplicationCore.Features.FivePieceOrderRelease;

public record FivePieceOrder(DateTime OrderDate,
                                DateTime DueDate,
                                string CompanyName,
                                string TrackingNumber,
                                string JobName,
                                string Material,
                                IEnumerable<LineItem> LineItems);
