using ApplicationCore.Features.CNC.Contracts;
using ApplicationCore.Features.CNC.ReleasePDF.Configuration;
using ApplicationCore.Features.CNC.ReleasePDF.PDFModels;
using ApplicationCore.Features.CNC.ReleasePDF.Styling;
using ApplicationCore.Shared;
using Microsoft.Extensions.Options;

namespace ApplicationCore.Features.CNC.ReleasePDF.Services;

internal class ReleasePDFDecoratorFactory {

    private readonly PDFConfiguration _config;

    public ReleasePDFDecoratorFactory(IOptions<PDFConfiguration> config) {
        _config = config.Value;
    }

    public IEnumerable<IDocumentDecorator> GenerateDecorators(ReleasedJob job) {

        List<IDocumentDecorator> decorators = new();

        var groups = job.Releases.GroupBy(r => r, new ReleaseGroupComparer());

        foreach (var group in groups) {

            CoverModel cover = CreateCover(job, group);
            List<PageModel> pages = CreatePages(job, group);

            decorators.Add(new ReleasePDFDecorator(_config, cover, pages));

        }

        return decorators;

    }

    private static List<PageModel> CreatePages(ReleasedJob job, IEnumerable<MachineRelease> releases) {

        var pages = new List<PageModel>();
        int programIndex = 0;
        foreach (var program in releases.First().Programs) {

            Dictionary<string, SheetProgam> programs = new();
            foreach (var release in releases) {
                var curr = release.Programs.ElementAt(programIndex);
                programs.Add(
                    release.MachineName,
                    new() {
                        Face5Program = curr.Name,
                        Face6Program = program.HasFace6 ? $"6{curr.Name[1..]}" : null
                    }
                );

            }

            var face6FileNames = program.Parts
                                        .Select(p => p.Face6FileName)
                                        .Where(f => f is not null).Cast<string>();

            var partGroups = program.Parts
                                    .Where(p => !face6FileNames.Contains(p.FileName))
                                    .OrderBy(p => p.ProductNumber)
                                    .GroupBy(p => p.PartId);

            bool containsFace6 = face6FileNames.Any();

            var partsTableContent = new List<Dictionary<string, string>>();
            foreach (var group in partGroups) {
                var part = group.First();
                var fields = new Dictionary<string, string> {
                        { "Product", part.ProductNumber },
                        { "Qty", group.Count().ToString() },
                        { "Name", part.Name },
                        { "Width", part.Width.AsMillimeters().ToString("0.00") },
                        { "Length", part.Length.AsMillimeters().ToString("0.00") },
                        { "Description", part.Description },
                        { "File Name", part.FileName }
                    };
                if (containsFace6) fields.Add("Face 6", part.Face6FileName ?? "");
                partsTableContent.Add(fields);
            }

            var material = program.Material;

            // TODO: add an option to use the file name or the line number (in the pattern)
            //int index = 1;
            var imgtxts = program.Parts.Select(p => new ImageText() { Text = $"{p.ProductNumber}-{p.FileName}", Location = p.Center });
            byte[] imageData = PatternImageFactory.CreatePatternImage(program.ImagePath, releases.First().MachineTableOrientation, program.Material.Width, program.Material.Length, imgtxts);

            pages.Add(new() {
                Header = $"{job.JobName}  [{string.Join(',', releases.Select(r => r.MachineName))}]",
                MachinePrograms = programs,
                Subtitle = $"{material.Name} - {material.Width:0.00}x{material.Length:0.00}x{material.Thickness:0.00}",

                TimeStamp = job.TimeStamp,
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

        var usedmaterials = releases.First()
                                    .Programs
                                    .Select(p => p.Material)
                                    .GroupBy(m => (m.Name, m.Width, m.Length, m.Thickness, m.IsGrained));
        var materialTableContent = new List<Dictionary<string, string>>();
        foreach (var mat in usedmaterials) {
            materialTableContent.Add(new() {
                    { "Qty", mat.Count().ToString() },
                    { "Name", mat.Key.Name },
                    { "Width", mat.Key.Width.ToString("0.00") },
                    { "Length", mat.Key.Length.ToString("0.00") },
                    { "Thickness", mat.Key.Thickness.ToString("0.00") },
                });
        }

        var materialTable = new Table() {
            Title = "Materials Used",
            Content = materialTableContent
        };

        var releasedparts = releases.First()
                                    .SinglePrograms
                                    .OrderBy(p => p.ProductNumber)
                                    .GroupBy(p => p.PartId);
        var partsTableContent = new List<Dictionary<string, string>>();
        foreach (var group in releasedparts) {
            var part = group.First();
            partsTableContent.Add(new() {
                    { "#", part.ProductNumber },
                    { "Name", part.Name },
                    { "FileName", part.FileName },
                    { "Width", part.Width.AsMillimeters().ToString("0.00") },
                    { "Length", part.Length.AsMillimeters().ToString("0.00") },
                });
        }

        var partsTable = new Table() {
            Title = "Single Parts",
            Content = partsTableContent
        };

        var toolTables = CreateToolTables(releases);

        // TODO: this might not work right with cabinets or other products that have multiple parts
        var twoSidedPartGroups = releases.First()
                                        .SinglePrograms
                                        .GroupBy(part => part.ProductNumber)
                                        .Where(group => group.Count() == 2)
                                        .Where(group => group.Any(p => p.HasBackSideProgram))
                                        .Select(group => group.ToArray());
        var backSideMachiningTable = CreateBackSideMachiningTable(twoSidedPartGroups);

        var tables = new List<Table>();
        tables.AddRange(toolTables);
        tables.Add(materialTable);
        tables.Add(partsTable);
        if (backSideMachiningTable != null) {
            tables.Add(backSideMachiningTable);
        }

        var cover = new CoverModel() {
            Title = $"{job.JobName}  [{string.Join(',', releases.Select(r => r.MachineName))}]",
            TimeStamp = job.TimeStamp,
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

    private static List<Table> CreateToolTables(IEnumerable<MachineRelease> releases) {

        List<Table> toolTables = new();

        foreach (var release in releases) {

            bool hasTools = false;

            var toolTableContent = new List<Dictionary<string, string>>();
            var row = new Dictionary<string, string>();

            foreach (var pos in release.ToolTable.Keys.OrderBy(p => p)) {
                row.Add(pos.ToString(), release.ToolTable[pos]);
                if (!string.IsNullOrWhiteSpace(release.ToolTable[pos])) {
                    hasTools = true;
                }
            }

            toolTableContent.Add(row);

            var toolTable = new Table() {
                Title = $"{release.MachineName} Tools Used",
                Content = toolTableContent
            };

            if (hasTools) {
                toolTables.Add(toolTable);
            }

        }

        return toolTables;

    }

    private static Table? CreateBackSideMachiningTable(IEnumerable<SinglePartProgram[]> twoSidedParts) {

        if (!twoSidedParts.Any()) return null;

        List<Dictionary<string, string>> content = new();

        twoSidedParts.ForEach(group => {

            if (group.Length != 2) {
                return;
            }

            var sideA = group[0];
            var sideB = group[1];

            content.Add(new() {
                { "#", sideA.ProductNumber },
                { "SideA", sideA.FileName },
                { "SideB", sideB.FileName }
            });

        });

        return new Table() {
            Title = "Back Side Machining",
            Content = content
        };

    }

}

