using CADCodeProxy.Machining;

namespace Domain.Orders;

public interface ICNCPartContainer {

    IEnumerable<Part> GetCNCParts();

    bool ContainsCNCParts();

}
