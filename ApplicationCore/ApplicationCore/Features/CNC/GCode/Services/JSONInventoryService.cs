using ApplicationCore.Features.CNC.GCode.Domain.Inventory;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplicationCore.Features.CNC.GCode.Services;

public class JSONInventoryService : IInventoryService
{

    private readonly string _fileName;

    public JSONInventoryService(string fileName)
    {
        _fileName = fileName;
    }

    public async Task<IEnumerable<InventoryItem>> GetInventory()
    {

        using var stream = File.OpenRead(_fileName);
        var inventory = await JsonSerializer.DeserializeAsync<List<InventoryItemModel>>(stream);

        if (inventory is null) return new List<InventoryItem>();

        return inventory.Select(i =>
        {

            var sizes = i.Sizes.Select(s => new InventorySize()
            {
                Width = s.Width,
                Length = s.Length,
                Priority = s.Priority
            });

            return new InventoryItem()
            {
                Name = i.Name,
                Thickness = i.Thickness,
                IsGrained = i.IsGrained,
                Sizes = sizes
            };

        });

    }

    class InventoryItemModel
    {

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("thickness")]
        public double Thickness { get; set; }

        [JsonPropertyName("isGrained")]
        public bool IsGrained { get; set; }

        [JsonPropertyName("sizes")]
        public List<InventorySizeModel> Sizes { get; set; } = new();

    }

    class InventorySizeModel
    {

        [JsonPropertyName("width")]
        public float Width { get; set; }

        [JsonPropertyName("length")]
        public float Length { get; set; }

        [JsonPropertyName("priority")]
        public int Priority { get; set; }

    }

}
