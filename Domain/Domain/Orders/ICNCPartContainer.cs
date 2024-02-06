using CADCodeProxy.Machining;

namespace Domain.Orders;

public interface ICNCPartContainer {

    IEnumerable<Part> GetCNCParts(string customerName);

    bool ContainsCNCParts();

}
