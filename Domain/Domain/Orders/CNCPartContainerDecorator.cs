using CADCodeProxy.Machining;
using Domain.Orders.Entities;

namespace Domain.Orders;

public class CNCPartContainerDecorator : ICNCPartContainer {

    private readonly ICNCPartContainer _partContainer;
    private readonly int _sequenceNumber;
    private readonly Order _order;
    private readonly string _customerName;

    public CNCPartContainerDecorator(ICNCPartContainer partContainer, int sequenceNumber, Order order, string customerName) {
        _partContainer = partContainer;
        _sequenceNumber = sequenceNumber;
        _order = order;
        _customerName = customerName;
    }

    public bool ContainsCNCParts() => _partContainer.ContainsCNCParts();

    public IEnumerable<Part> GetCNCParts() {

        foreach (var part in _partContainer.GetCNCParts()) {

            part.PrimaryFace.ProgramName = $"{_sequenceNumber}{_order.Number}{part.PrimaryFace.ProgramName}";

            if (part.SecondaryFace is not null) {
                part.PrimaryFace.ProgramName = $"{_sequenceNumber}{_order.Number}{part.PrimaryFace.ProgramName}";
            }

            part.InfoFields.Add("CustomerInfo1", _customerName);

            yield return part;

        }

    }

}
