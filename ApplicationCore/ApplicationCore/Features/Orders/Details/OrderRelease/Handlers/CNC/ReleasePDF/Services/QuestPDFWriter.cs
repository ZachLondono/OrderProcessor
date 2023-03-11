using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Configuration;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Contracts;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.PDFModels;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Styling;

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

        foreach (var release in job.Releases) {

            CoverModel cover = CreateCover(job, release);
            List<PageModel> pages = CreatePages(job, release);

            decorators.Add(new ReleasePDFDecorator(config, cover, pages));

        }

        return decorators;

    }

    private static List<PageModel> CreatePages(ReleasedJob job, MachineRelease release) {

        var pages = new List<PageModel>();
        foreach (var program in release.Programs) {

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
            byte[] imageData = PatternImageFactory.CreatePatternImage(program.ImagePath, release.MachineTableOrientation, program.Material.Width, program.Material.Length, imgtxts);

            pages.Add(new() {
                Header = $"{job.JobName}  [{release.MachineName}]",
                Title = program.Name,
                Title2 = program.HasFace6 ? $"6{program.Name.Remove(0, 1)}" : "",
                Subtitle = $"{material.Name} - {material.Width:0.00}x{material.Length:0.00}x{material.Thickness:0.00} (grained:{(material.IsGrained ? "yes" : "no")})",

                Footer = "footer",
                ImageData = imageData,
                Parts = new Table() {
                    Title = "Parts on Sheet",
                    Content = partsTableContent
                },
            });

        }

        return pages;
    }

    private static CoverModel CreateCover(ReleasedJob job, MachineRelease release) {
        var usedmaterials = release.Programs.Select(p => p.Material).GroupBy(m => (m.Name, m.Width, m.Length, m.Thickness, m.IsGrained));
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

        var releasedparts = release.Programs.SelectMany(p => p.Parts).OrderBy(p => p.ProductNumber).GroupBy(p => p.Name);
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
        foreach (var pos in release.ToolTable.Keys.OrderBy(p => p)) {
            row.Add(pos.ToString(), release.ToolTable[pos]);
            if (!string.IsNullOrWhiteSpace(release.ToolTable[pos])) hasTools = true;
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
            Title = $"{job.JobName}  [{release.MachineName}]",
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

    private static string GetFileName(string path, string filename) {

        int num = 0;

        string fullpath = Path.Combine(path, $"{filename}.pdf");
        while (File.Exists(fullpath)) {
            fullpath = Path.Combine(path, $"{filename} ({++num}).pdf");
        }

        return fullpath;

    }

}
