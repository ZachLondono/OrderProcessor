using CADCodeProxy.Machining;

namespace ApplicationCore.Features.Orders.Shared.Domain;

internal interface ICNCPartContainer {

    IEnumerable<Part> GetCNCParts();

    bool ContainsCNCParts();

}
