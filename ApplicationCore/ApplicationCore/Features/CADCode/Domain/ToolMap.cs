namespace ApplicationCore.Features.CADCode.Services.Domain;

public class ToolMap {
    
    public int SpindleCount { get; init; }
    public IList<DrillBit> DrillBlock { get; set; } = new List<DrillBit>();

    private readonly Dictionary<int, List<RouterBit>> _spindles = new();
    public IReadOnlyDictionary<int, IEnumerable<RouterBit>> Spindles => (IReadOnlyDictionary<int, IEnumerable<RouterBit>>)_spindles;

    private readonly Dictionary<string, (RouterBit, int)> _toolsByName = new();
    public IReadOnlyDictionary<string, (RouterBit,int)> Tools => _toolsByName;

    public ToolMap(int spindleCount) {
        SpindleCount = spindleCount;
    }

    public bool AddTool(int position, RouterBit tool) {

        // Every tool in the tool map must have a unique name
        // Each tool must be in a valid position from 1 to SpindleCount

        if (position > SpindleCount || position < 1) return false;

        if (_toolsByName.ContainsKey(tool.Name)) return false;

        if (_spindles.ContainsKey(position)) {
            if (_spindles[position].Any(t => t.Name.Equals(tool.Name))) return false;
            _spindles[position].Add(tool);
            return true;
        }

        _toolsByName.Add(tool.Name, (tool, position));
        _spindles[position] = new() { tool };

        return true;
    }

}
