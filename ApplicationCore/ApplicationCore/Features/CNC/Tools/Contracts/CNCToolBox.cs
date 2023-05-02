namespace ApplicationCore.Features.CNC.Tools.Contracts;

public static class CNCToolBox {

    public delegate Task<IEnumerable<ToolCarousel>> GetToolCarousels();

}
