using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Persistance.DataModels;

public class AdditionalOpeningDataModel {

    public Dimension RailWidth { get; set; }
    public Dimension OpeningHeight { get; set; }
    public bool IsOpenPanel { get; set; }
    public bool RabbetBack { get; set; }
    public bool RouteForGasket { get; set; }

    public AdditionalOpening ToAdditionalOpening() => new(RailWidth, OpeningHeight, IsOpenPanel ? new OpenPanel(RabbetBack, RouteForGasket) : new SolidPanel());

}
