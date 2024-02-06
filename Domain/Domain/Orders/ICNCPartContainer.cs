using CADCodeProxy.Machining;

namespace Domain.Orders;

internal interface ICNCPartContainer {

    IEnumerable<Part> GetCNCParts(string customerName);

    bool ContainsCNCParts();

}
