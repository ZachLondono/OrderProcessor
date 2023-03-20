using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Configuration;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Contracts;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.PDFModels;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Styling;
using System.Diagnostics.CodeAnalysis;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Services;

internal class QuestPDFWriter : IReleasePDFWriter {

    private readonly IPDFConfigurationProvider _configProvider;

    public QuestPDFWriter(IPDFConfigurationProvider configProvider) {
        _configProvider = configProvider;
    }

    public IEnumerable<IDocumentDecorator> GenerateDecorators(ReleasedJob job) {

        // TODO: validate configuration
        var config = _configProvider.GetConfiguration();

        List<IDocumentDecorator> decorators = new();

        var groups = job.Releases.GroupBy(r => r, new ReleaseGroupComparer());

        foreach (var group in groups) {

            CoverModel cover = CreateCover(job, group);
            List<PageModel> pages = CreatePages(job, group);

            decorators.Add(new ReleasePDFDecorator(config, cover, pages));

        }

        return decorators;

    }

    private static List<PageModel> CreatePages(ReleasedJob job, IEnumerable<MachineRelease> releases) {

        var pages = new List<PageModel>();
        int programIndex = 0;
        foreach (var program in releases.First().Programs) {

            List<ReleasedProgram> programs = new();
            foreach (var release in releases) {
                programs.Add(release.Programs.ElementAt(programIndex));
            }

            var partsTableContent = new List<Dictionary<string, string>>();
            var partGroups = program.Parts.GroupBy(p => p.Name);
            foreach (var group in partGroups) {
                var part = group.First();
                partsTableContent.Add(new()  {
                        { "Product", part.ProductNumber },
                        { "Qty", group.Count().ToString() },
                        { "Name", part.Name },
                        { "Width", part.Width.AsMillimeters().ToString("0.00") },
                        { "Length", part.Length.AsMillimeters().ToString("0.00") },
                        { "Description", part.Description }
                    });
            }

            var material = program.Material;

            // TODO: add an option to use the file name or the line number (in the pattern)
            //int index = 1;
            var imgtxts = program.Parts.Select(p => new ImageText() { Text = $"{p.Name}", Location = p.Center });
            byte[] imageData = PatternImageFactory.CreatePatternImage(program.ImagePath, releases.First().MachineTableOrientation, program.Material.Width, program.Material.Length, imgtxts);

            pages.Add(new() {
                Header = $"{job.JobName}  [{string.Join(',', releases.Select(r => r.MachineName))}]",
                Title = string.Join(" | ", programs.Select(p => p.Name)),
                Title2 = program.HasFace6 ? string.Join(" | ", programs.Select(p => $"6{p.Name[1..]}" )) : "",
                Subtitle = $"{material.Name} - {material.Width:0.00}x{material.Length:0.00}x{material.Thickness:0.00} (grained:{(material.IsGrained ? "yes" : "no")})",

                Footer = "footer",
                ImageData = imageData,
                Parts = new Table() {
                    Title = "Parts on Sheet",
                    Content = partsTableContent
                },
            });

            programIndex++;

        }

        return pages;
    }

    private static CoverModel CreateCover(ReleasedJob job, IEnumerable<MachineRelease> releases) {

        if (!releases.Any()) return new();

        var usedmaterials = releases.First().Programs.Select(p => p.Material).GroupBy(m => (m.Name, m.Width, m.Length, m.Thickness, m.IsGrained));
        var materialTableContent = new List<Dictionary<string, string>>();
        foreach (var mat in usedmaterials) {
            materialTableContent.Add(new() {
                    { "Qty", mat.Count().ToString() },
                    { "Name", mat.Key.Name },
                    { "Width", mat.Key.Width.ToString("0.00") },
                    { "Length", mat.Key.Length.ToString("0.00") },
                    { "Thickness", mat.Key.Thickness.ToString("0.00") },
                    { "Grained", mat.Key.IsGrained ? "yes" : "no" },
                });
        }

        var materialTable = new Table() {
            Title = "Materials Used",
            Content = materialTableContent
        };

        var releasedparts = releases.First().Programs.SelectMany(p => p.Parts).OrderBy(p => p.ProductNumber).GroupBy(p => p.Name);
        var partsTableContent = new List<Dictionary<string, string>>();
        foreach (var group in releasedparts) {
            var part = group.First();
            partsTableContent.Add(new() {
                    { "Product", part.ProductNumber },
                    { "Qty", group.Count().ToString() },
                    { "Name", part.Name },
                    { "Width", part.Width.AsMillimeters().ToString("0.00") },
                    { "Length", part.Length.AsMillimeters().ToString("0.00") },
                });
        }

        var partsTable = new Table() {
            Title = "Parts in Release",
            Content = partsTableContent
        };

        var toolTableContent = new List<Dictionary<string, string>>();
        var row = new Dictionary<string, string>();
        bool hasTools = false;
        foreach (var pos in releases.First().ToolTable.Keys.OrderBy(p => p)) {
            row.Add(pos.ToString(), releases.First().ToolTable[pos]);
            if (!string.IsNullOrWhiteSpace(releases.First().ToolTable[pos])) hasTools = true;
        }
        toolTableContent.Add(row);

        var toolTable = new Table() {
            Title = "Tools Used",
            Content = toolTableContent
        };

        var tables = new List<Table>();
        if (hasTools) tables.Add(toolTable);
        tables.Add(materialTable);
        tables.Add(partsTable);

        string workIdStr = job.WorkOrderId is null ? "" : GetGuidAsBase64((Guid)job.WorkOrderId);

        var cover = new CoverModel() {
            Title = $"{job.JobName}  [{string.Join(',', releases.Select(r => r.MachineName))}]",
            WorkOrderId = workIdStr,
            Info = new Dictionary<string, string>() {
                    {"Vendor", job.VendorName },
                    {"Customer", job.CustomerName },
                    {"Order Date", job.OrderDate.ToShortDateString() },
                    {"Release Date", job.ReleaseDate.ToShortDateString() }
                },
            Tables = tables
        };

        return cover;
    }

    private static string GetGuidAsBase64(Guid id) => Convert.ToBase64String(id.ToByteArray()).Replace("/", "-").Replace("+", "_").Replace("=", "");

    private class ReleaseGroupComparer : IEqualityComparer<MachineRelease> {
        
        public bool Equals(MachineRelease? x, MachineRelease? y) {
            
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;

            var xEnum = x.Programs.GetEnumerator();
            var yEnum = y.Programs.GetEnumerator();

            while (true) {

                bool xResult = xEnum.MoveNext();
                bool yResult = yEnum.MoveNext();

                if (xResult != yResult) return false;
                if (!xResult) break;

                var xProg = xEnum.Current;
                var yProg = yEnum.Current;

                if (xProg.Material.Name != yProg.Material.Name) return false;
                if (xProg.Material.Width != yProg.Material.Width) return false;
                if (xProg.Material.Length != yProg.Material.Length) return false;
                if (xProg.Material.Thickness != yProg.Material.Thickness) return false;

                var xParts = xProg.Parts.GroupBy(p => p.Name).Select(g => new PartGroup(g.Key, g.Count()));
                var yParts = yProg.Parts.GroupBy(p => p.Name).Select(g => new PartGroup(g.Key, g.Count()));

                if (!xParts.All(yParts.Contains)) return false;

            }

            if (x.ToolTable.Keys.Count() == y.ToolTable.Keys.Count() && x.ToolTable.Keys.All(k => y.ToolTable.ContainsKey(k) && object.Equals(y.ToolTable[k], x.ToolTable[k]))) return true;

            return false;

        }

        public int GetHashCode([DisallowNull] MachineRelease obj) => 0;

        public record PartGroup(string Name, int Count);

    }

}
