using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport.Contracts;
using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport.Domain;
using MoreLinq;

namespace ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport.Services;

public class PPJobConverter {

    private readonly IExtWriter _writer;

    public PPJobConverter(IExtWriter writer) {
        _writer = writer;
    }

    public void ConvertOrder(PPJob job) {

        _writer.Clear();

        var rooms = job.Products
                        .GroupBy(prod => prod.Room)
                        .Select(group =>
                            new Room() {
                                Name = group.Key,

                                // products within the room are grouped together by MaterialGroupKey
                                Groups = group.GroupBy(prod => new MaterialGroupKey(prod.Catalog, prod.MaterialType, prod.DoorType, prod.HardwareType, prod.FinishMaterials, prod.EBMaterials), new MaterialGroupKeyComparer())
                                                .Select(group => new MaterialGroup() {
                                                    Key = group.Key,
                                                    Products = group.ToList()
                                                })
                                                .ToList()
                            });


        int jobId = 0;

        var jobDesc = new JobDescriptor() {
            LevelId = jobId,
            Job = job.Name,
            Date = job.OrderDate,
            Catalog = "",
            Fronts = "",
            Hardware = "",
            Materials = ""
        };

        _writer.AddRecord(jobDesc);

        // If there is only one unnamed room in the order, there does not need to be a level for the room, instead just skip to adding group levels
        if (rooms.Count() == 1 && string.IsNullOrWhiteSpace(rooms.First().Name)) {

            var room = rooms.First();

            int groupIdx = 0;
            foreach (var group in room.Groups) {

                groupIdx++;
                AddGroupToWriter(group, jobId, jobId + groupIdx, $"Lvl{groupIdx}");

            }

            return;

        }

        int roomIdx = 0;
        foreach (var room in rooms) {

            if (!room.Groups.Any()) {
                continue;
            }

            roomIdx++;

            var firstGroup = room.Groups.First();

            // Every level must have catalog, material, fronts & hardware defined, even if there are no products directly within it
            var level = new LevelDescriptor() {
                LevelId = jobId + roomIdx,
                ParentId = jobId,
                Name = string.IsNullOrEmpty(room.Name) ? $"Lvl{roomIdx}" : room.Name,

                // The from the first group are used, since if there is only one group in the room the products will be added directly to this level
                Catalog = firstGroup.Key.Catalog,
                Materials = firstGroup.Key.MaterialType,
                Fronts = firstGroup.Key.DoorType,
                Hardware = firstGroup.Key.HardwareType,
            };

            _writer.AddRecord(level);

            if (room.Groups.Count() == 1) {

                // If there is only one group in the level, add it directly to this level rather than creating sub levels

                AddMaterialVariablesToWriter(firstGroup.Key, level.LevelId);

                foreach (var product in firstGroup.Products) {
                    AddProductToWriter(product, jobId + roomIdx);
                }

                continue;

            } else { 

                int groupIdx = 0;
                foreach (var group in room.Groups) {

                    groupIdx++;

                    int lvlId = jobId + roomIdx + groupIdx;
                    AddGroupToWriter(group, jobId, lvlId, $"{groupIdx}-{level.Name}");

                }

                // Increment the roomIdx by the number of groups, so that subsequent rooms are not given the same level id as a sub group 
                roomIdx += groupIdx;

            }

        }

    }

    /// <summary>
    /// Adds a level descriptor for the group, a variable descriptor which holds the variables in the group key and the product descriptors for all products in the group.
    /// </summary>
    private void AddGroupToWriter(MaterialGroup group, int parentId, int levelId, string name) {

        var subLevel = new LevelDescriptor() {
            LevelId = levelId,
            ParentId = parentId,
            Name = name,
            Catalog = group.Key.Catalog,
            Materials = group.Key.MaterialType,
            Fronts = group.Key.DoorType,
            Hardware = group.Key.HardwareType
        };

        _writer.AddRecord(subLevel);

        AddMaterialVariablesToWriter(group.Key, subLevel.LevelId);

        foreach (var product in group.Products) {
            AddProductToWriter(product, subLevel.LevelId);
        }

    }

    private void AddMaterialVariablesToWriter(MaterialGroupKey materials, int levelId) {

        var overrides = new VariableOverride() {
            LevelId = levelId,
            Units = PPUnits.Millimeters,
            Parameters = materials.AllMaterials
        };

        _writer.AddRecord(overrides);

    }

    private void AddProductToWriter(PPProduct product, int parentLevelId) {

        if (product.OverrideParameters.Any()) {

            var overrides = new VariableOverride() {
                LevelId = parentLevelId,
                Units = PPUnits.Millimeters,
                Parameters = product.OverrideParameters
            };

            _writer.AddRecord(overrides);

        }

        var prodRec = new ProductRecord() {
            Name = product.Name,
            ParentId = parentLevelId,
            Pos = product.SequenceNum,
            CustomSpec = true,
            Qty = product.Qty,
            SeqText = "",
            Units = PPUnits.Millimeters,
            Comment = product.Comment,
            ProductId = product.ProductId,
            Parameters = product.Parameters
        };

        _writer.AddRecord(prodRec);

    }

    public static bool AreDictionariesEquivalent(IDictionary<string, string>? x, IDictionary<string, string>? y) {
        if (x is null && y is null) return true;
        if (x is null && y is not null || y is null && x is not null) return false;
        if (x!.Count != y!.Count) return false;

        foreach (var (key, value) in x) {

            if (!y.ContainsKey(key)) return false;

            if (value != y[key]) return false;

        }

        return true;
    }

    class Room {
        public required string Name { get; set; }
        public required IEnumerable<MaterialGroup> Groups { get; set; }
    }

    class MaterialGroup {
        public required MaterialGroupKey Key { get; set; }
        public required IEnumerable<PPProduct> Products { get; set; }
    }

    class MaterialGroupKey {

        public string Catalog { get; }
        public string MaterialType { get; }
        public string DoorType { get; }
        public string HardwareType { get; }
        public IDictionary<string, PPMaterial> FinishMaterials { get; }
        public IDictionary<string, PPMaterial> EBMaterials { get; }
        public IDictionary<string, string> AllMaterials { get; }

        public MaterialGroupKey(string catalog, string materialType, string doorType, string hardwareType, IDictionary<string, PPMaterial> finishMaterials, IDictionary<string, PPMaterial> ebMaterials) {
            Catalog = catalog;
            MaterialType = materialType;
            DoorType = doorType;
            HardwareType = hardwareType;
            FinishMaterials = finishMaterials;
            EBMaterials = ebMaterials;

            AllMaterials = new Dictionary<string, string>();
            FinishMaterials.ForEach(mat => AllMaterials.Add(mat.Key, mat.Value.ToString()));
            EBMaterials.ForEach(mat => AllMaterials.Add(mat.Key, mat.Value.ToString()));
        }

    }

    class MaterialGroupKeyComparer : IEqualityComparer<MaterialGroupKey> {
        public bool Equals(MaterialGroupKey? x, MaterialGroupKey? y) {
            if (x is null || y is null) return false;

            if (x.Catalog != y.Catalog || x.MaterialType != y.MaterialType || x.DoorType != y.DoorType || x.HardwareType != y.HardwareType) return false;
            if (AreDictionariesEquivalent(y.AllMaterials, x.AllMaterials)) return true;
            return false;
        }

        public int GetHashCode(MaterialGroupKey obj) => 0;
    }

    class DictionaryValueComparer : IEqualityComparer<IDictionary<string, string>> {

        public bool Equals(IDictionary<string, string>? x, IDictionary<string, string>? y) {
            return AreDictionariesEquivalent(x, y);
        }

        public int GetHashCode(IDictionary<string, string> obj) => 0;

    }

}
