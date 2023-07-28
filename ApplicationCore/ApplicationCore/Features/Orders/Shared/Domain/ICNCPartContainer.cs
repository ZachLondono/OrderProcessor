using CADCodeProxy.Machining;

namespace ApplicationCore.Features.Orders.Shared.Domain;

internal interface ICNCPartContainer {

    IEnumerable<Part> GetCNCParts(string customerName);

    bool ContainsCNCParts();

}
