namespace Domain.Orders.ValueObjects;

public record OpenPanel {

	public bool RabbetBack { get; init; }
	public bool RouteForGasket { get; init; }

    public OpenPanel(bool rabbetBack, bool routeForGasket) {
		RabbetBack = rabbetBack;
        RouteForGasket = routeForGasket;

        if (!RabbetBack && RouteForGasket) {
            throw new ArgumentException("A panel can not have a route for glass gasket if it does not have a rabbet.");
        }

    }

}
