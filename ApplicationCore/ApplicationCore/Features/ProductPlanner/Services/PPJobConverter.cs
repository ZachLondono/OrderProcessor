using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.ProductPlanner.Domain;
using MoreLinq;

namespace ApplicationCore.Features.ProductPlanner.Services;

public class PPJobConverter {

    public ExtWriter ConvertOrder(PPJob ppjob) {

        var writer = new ExtWriter();

        var rooms = ppjob.Products.GroupBy(prod => prod.Room);

        int jobId = 0;
        // TODO: if there are not multiple rooms or multiple different materials, all products can be put in the same top level job without any levels
        var job = new JobDescriptor() {
            LevelId = jobId,
            Job = ppjob.Name,
            Date = ppjob.OrderDate,
            Catalog = "",
            Fronts = "",
            Hardware = "",
            Materials = ""
        };

        writer.AddRecord(job);

        int roomIdx = 0;
        foreach (var room in rooms) {

            if (!room.Any()) continue;

            roomIdx++;

            var materialGroups = room.GroupBy(prod => new MaterialGroupKey(prod.Catalog, prod.MaterialType, prod.DoorType, prod.HardwareType, prod.FinishMaterials, prod.EBMaterials), new MaterialGroupKeyComparer());
            bool multipleMaterials = materialGroups.Count() > 1;

            var firstMaterials = materialGroups.First().Key;
            var level = new LevelDescriptor() {
                LevelId = jobId + roomIdx,
                ParentId = jobId,
                Name = string.IsNullOrEmpty(room.Key) ? $"Lvl{roomIdx}" : room.Key,
                Catalog = firstMaterials.Catalog,
                Materials = firstMaterials.MaterialType,
                Fronts = firstMaterials.DoorType,
                Hardware = firstMaterials.HardwareType
            };

            if (!multipleMaterials) {
                writer.AddRecord(GetMaterialVariableRecord(firstMaterials.AllMaterials, jobId + roomIdx));
            }

            writer.AddRecord(level);


            int materialIdx = 0;
            foreach (var material in materialGroups) {

                materialIdx++;

                if (multipleMaterials) {
                    int lvlId = jobId + roomIdx + materialIdx;
                    writer.AddRecord(new LevelDescriptor() {
                        LevelId = lvlId,
                        ParentId = roomIdx,
                        Name = $"{materialIdx}-{(string.IsNullOrEmpty(room.Key) ? $"Lvl{roomIdx}" : room.Key)}",
                        Catalog = material.Key.Catalog,
                        Materials = material.Key.MaterialType,
                        Fronts = material.Key.DoorType,
                        Hardware = material.Key.HardwareType
                    });

                    writer.AddRecord(GetMaterialVariableRecord(material.Key.AllMaterials, lvlId));
                }

                if (material.Any(cab => cab.OverrideParameters.Any())) {

                    // TODO: if there are not multiple override groups, use the material level to set overrides

                    var overrideGroups = material.GroupBy(prod => prod.OverrideParameters, new DictionaryValueComparer());

                    int overrideIdx = 0;
                    foreach (var group in overrideGroups) {

                        overrideIdx++;

                        int parentId = jobId + roomIdx + (multipleMaterials ? materialIdx : 0);
                        int lvlId = parentId + overrideIdx;
                        writer.AddRecord(new LevelDescriptor() {
                            LevelId = lvlId,
                            ParentId = parentId,
                            Name = $"{(multipleMaterials ? $"{materialIdx}.{overrideIdx}" : overrideIdx.ToString())}-{(string.IsNullOrEmpty(room.Key) ? $"Lvl{roomIdx}" : room.Key)}",
                            Catalog = "",
                            Materials = "",
                            Fronts = "",
                            Hardware = ""
                        });

                        writer.AddRecord(GetMaterialVariableRecord(material.Key.AllMaterials, lvlId));

                        writer.AddRecord(new VariableOverride() {
                            LevelId = lvlId,
                            Units = PPUnits.Millimeters,
                            Parameters = group.Key
                        });

                        foreach (var prod in group) {
                            writer.AddRecord(MapProductToRecord(prod, lvlId));
                        }

                    }

                    materialIdx += overrideIdx;

                } else {

                    foreach (var prod in material) {
                        int parentId = jobId + roomIdx + (multipleMaterials ? materialIdx : 0);
                        writer.AddRecord(MapProductToRecord(prod, parentId));
                    }

                }

            }

            roomIdx += materialIdx;

        }

        return writer;

    }

    private VariableOverride GetMaterialVariableRecord(Dictionary<string, string> materials, int levelId) {
        var variables = new VariableOverride {
            Units = PPUnits.Millimeters,
            Parameters = materials,
            LevelId = levelId
        };

        return variables;
    }

    private static ProductRecord MapProductToRecord(PPProduct product, int parentId) => new() {
        Name = product.Name,
        ParentId = parentId,
        Pos = product.SequenceNum,
        CustomSpec = true,
        Qty = 1,
        SeqText = "",
        Units = PPUnits.Millimeters,
        Parameters = product.Parameters
    };

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
