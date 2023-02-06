using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.ProductPlanner.Domain;
using MoreLinq;

namespace ApplicationCore.Features.ProductPlanner.Services;

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
                                Groups = group.GroupBy(prod => new MaterialGroupKey(prod.Catalog, prod.MaterialType, prod.DoorType, prod.HardwareType, prod.FinishMaterials, prod.EBMaterials), new MaterialGroupKeyComparer())
                                                .Select(group => new MaterialGroup() {
                                                    Key = group.Key,
                                                    Products = group.ToList()
                                                })
                                                .ToList()
                        });


        int jobId = 0;
        int roomIdx = 0;

        var jobDesc = new JobDescriptor() {
            LevelId = jobId,
            Job = job.Name,
            Date = job.OrderDate,
            Catalog = "",
            Fronts = "",
            Hardware = "",
            Materials = ""
        };

        if (rooms.Count() == 1 && string.IsNullOrWhiteSpace(rooms.First().Name)) {

            var room = rooms.First();

            _writer.AddRecord(jobDesc);

            int groupIdx = 0;
            foreach (var group in room.Groups) {

                groupIdx++;
                AddGroupToWriter(group, jobId, roomIdx, groupIdx, $"Lvl{groupIdx}");

            }

            return;

        }

        _writer.AddRecord(jobDesc);

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
                Catalog = firstGroup.Key.Catalog,
                Materials = firstGroup.Key.MaterialType,
                Fronts = firstGroup.Key.DoorType,
                Hardware = firstGroup.Key.HardwareType,
            };

            _writer.AddRecord(level);

            if (room.Groups.Count() == 1) {

                AddMaterialVariablesToWriter(firstGroup.Key, level.LevelId);

                foreach (var product in firstGroup.Products) {
                    AddProductToWriter(product, jobId + roomIdx);
                }

                continue;

            }

            int groupIdx = 0;
            foreach (var group in room.Groups) {

                groupIdx++;

                AddGroupToWriter(group, jobId, roomIdx, groupIdx, $"{groupIdx}-{level.Name}");

            }

        }

    }

    private void AddGroupToWriter(MaterialGroup group, int jobId, int roomIdx, int groupIdx, string name) {

        var subLevel = new LevelDescriptor() {
            LevelId = jobId + roomIdx + groupIdx,
            ParentId = jobId + roomIdx,
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
            Qty = 1,
            SeqText = "",
            Units = PPUnits.Millimeters,
            Comment = product.Comment,
            ProductId = product.ProductId,
            Parameters = product.Parameters
        };

        _writer.AddRecord(prodRec);

    }

    public static bool AreDictionariesEquivalent(Dictionary<string, string>? x, Dictionary<string, string>? y) {
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
        public Dictionary<string, PPMaterial> FinishMaterials { get; }
        public Dictionary<string, PPMaterial> EBMaterials { get; }
        public Dictionary<string, string> AllMaterials { get; }

        public MaterialGroupKey(string catalog, string materialType, string doorType, string hardwareType, Dictionary<string, PPMaterial> finishMaterials, Dictionary<string, PPMaterial> ebMaterials) {
            Catalog = catalog;
            MaterialType = materialType;
            DoorType = doorType;
            HardwareType = hardwareType;
            FinishMaterials = finishMaterials;
            EBMaterials = ebMaterials;

            AllMaterials = new();
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

    class DictionaryValueComparer : IEqualityComparer<Dictionary<string, string>> {

        public bool Equals(Dictionary<string, string>? x, Dictionary<string, string>? y) {
            return AreDictionariesEquivalent(x, y);
        }

        public int GetHashCode(Dictionary<string, string> obj) => 0;

    }

}
