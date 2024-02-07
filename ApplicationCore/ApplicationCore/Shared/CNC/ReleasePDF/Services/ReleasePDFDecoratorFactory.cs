using ApplicationCore.Shared.CNC.Job;
using ApplicationCore.Shared.CNC.ReleasePDF.PDFModels;
using ApplicationCore.Shared.CNC.ReleasePDF.Styling;
using ApplicationCore.Shared.Settings;
using Microsoft.Extensions.Options;
using Windows.ApplicationModel;
using Domain.Extensions;

namespace ApplicationCore.Shared.CNC.ReleasePDF.Services;

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

        // If the releases contains a release for the Omnitech, use that one, otherwise just use the first one
        var releaseData = releases.First();
        if (releases.FirstOrDefault(r => r.MachineName.Contains("omni", StringComparison.InvariantCultureIgnoreCase)) is MachineRelease omniRelease) {
            releaseData = omniRelease;
        }

        var pages = new List<PageModel>();
        int programIndex = 0;
        foreach (var program in releaseData.Programs) {

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

            var parts = program.Parts
                               .Where(p => !face6FileNames.Contains(p.FileName))
                               .GroupBy(p => p.PartId)
                               .OrderBy(g => {
                                   if (int.TryParse(g.First().ProductNumber, out int productNumber)) return productNumber;
                                   return 0;
                               })
                               .ToArray();

            bool containsFace6 = face6FileNames.Any();
            bool containsBackSideProgram = parts.Any(group => group.Any(part => part.HasBackSideProgram));
            bool containsNote = parts.Any(group => group.Any(part => !string.IsNullOrWhiteSpace(part.Note)));

            var partsTableContent = new List<Dictionary<string, string>>();
            foreach (var group in parts) {
                var part = group.First();
                var fields = new Dictionary<string, string> {
                        { "Product", part.ProductNumber },
                        { "Qty", group.Count().ToString() },
                        { "Width", part.Width.AsMillimeters().ToString("0.00") },
                        { "Length", part.Length.AsMillimeters().ToString("0.00") },
                        { "File Name", part.FileName },
                        { "Description", part.Description }
                    };
                if (containsNote) fields.Add("Note", part.Note);
                if (containsFace6) fields.Add("Face 6", part.Face6FileName ?? "");
                if (containsBackSideProgram) fields.Add("Back Side", part.HasBackSideProgram ? "Y" : "");
                partsTableContent.Add(fields);
            }

            var material = program.Material;

            var imgtxts = program.Parts.Select(p => new ImageText() { Text = $"{p.ProductNumber}-{p.FileName}", Location = p.Center });
            byte[] imageData = PatternImageFactory.CreatePatternImage(program.ImagePath, releaseData.MachineTableOrientation, program.Material.Width, program.Material.Length, imgtxts);

            pages.Add(new() {
                Header = $"{job.JobName}  [{string.Join(',', releases.Select(r => r.MachineName))}]",
                MachinePrograms = programs,
                Subtitle = $"{material.Name} - {material.Width:0.00}x{material.Length:0.00}x{material.Thickness:0.00} - {material.Yield:P2}",

                TimeStamp = job.TimeStamp,
                ImageData = imageData,
                Parts = new Table() {
                    Title = $"Parts on Sheet ({parts.Sum(g => g.Count())})",
                    Content = partsTableContent,
                    ColumnWidths = new Dictionary<string, float>() {
                        { "Product", 50 },
                        { "Qty", 30 },
                        { "Width", 50 },
                        { "Length", 50 },
                        { "Note", 75 },
                    }
                },
            });

            programIndex++;

        }

        return pages;
    }

    private static CoverModel CreateCover(ReleasedJob job, IEnumerable<MachineRelease> releases) {

        if (!releases.Any()) return new();

        Table materialTable = CreateMaterialsTable(releases);

        Table programsTable = CreateProgramsTable(releases);

        Table partsTable = CreatePartsTable(releases);

        var toolTables = CreateToolTables(releases);

        Table? backSideMachiningTable = CreateBackSideMachiningTable(releases);

        var tables = new List<Table>();
        tables.AddRange(toolTables);
        if (materialTable.Content.Any()) tables.Add(materialTable);
        tables.Add(programsTable);
        if (partsTable.Content.Any()) tables.Add(partsTable);
        if (backSideMachiningTable != null) {
            tables.Add(backSideMachiningTable);
        }

        var coverInfo = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(job.VendorName)) coverInfo.Add("Vendor", job.VendorName);
        if (!string.IsNullOrWhiteSpace(job.CustomerName)) coverInfo.Add("Customer", job.CustomerName);
        coverInfo.Add("Order Date", job.OrderDate.ToShortDateString());
        coverInfo.Add("Release Date", job.ReleaseDate.ToShortDateString());

        if (job.DueDate is DateTime dueDate) {
            coverInfo.Add("Due Date", dueDate.ToShortDateString());
        }

        var cover = new CoverModel() {
            Title = $"{job.JobName}  [{string.Join(',', releases.Select(r => r.MachineName))}]",
            TimeStamp = job.TimeStamp,
            ApplicationVersion = GetApplicationVersion(),
            Info = coverInfo,
            Tables = tables
        };

        return cover;
    }

    private static string GetApplicationVersion() {

        try {
            var version = Package.Current.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        } catch { }

        return "0.0.0";

    }

    private static Table? CreateBackSideMachiningTable(IEnumerable<MachineRelease> releases) {

        // TODO: this will not work if there is another part/product with the same product number (e.g. combining two orders or a cabinet with multiple parts)
        var content = releases.First()
                              .SinglePrograms
                              .GroupBy(part => part.ProductNumber)
                              //.Where(group => group.Count() == 2)
                              .Where(group => group.Any(p => p.HasBackSideProgram))
                              .Select(group => (group.First(), group.Skip(1).First()))
                              .Select(group => new Dictionary<string, string>() {
                              { "#", group.Item1.ProductNumber },
                              { "SideA", group.Item1.FileName },
                              { "SideB", group.Item2.FileName }
                              }).ToList();

        if (!content.Any()) return null;

        return new Table() {
            Title = "Back Side Machining",
            Content = content
        };

    }

    private static Table CreatePartsTable(IEnumerable<MachineRelease> releases) {

        var uniqueParts = releases.First()
                                  .SinglePrograms
                                  .GroupBy(p => p.PartId).Select(g => g.First())
                                  .OrderBy(p => {
                                      if (int.TryParse(p.ProductNumber, out int productNumber)) return productNumber;
                                      return 0;
                                  })
                                  .ToArray();

        var partsTableContent = new List<Dictionary<string, string>>();
        foreach (var part in uniqueParts) {
            partsTableContent.Add(new() {
                    { "#", part.ProductNumber },
                    { "FileName", part.FileName },
                    { "Description", part.Description },
                    { "Width", part.Width.AsMillimeters().ToString("0.00") },
                    { "Length", part.Length.AsMillimeters().ToString("0.00") },
                });
        }

        var partsTable = new Table() {
            Title = $"Single Parts ({uniqueParts.Length} unique)",
            Content = partsTableContent,
            ColumnWidths = new Dictionary<string, float> {
                { "#", 50 },
                { "Width", 50 },
                { "Length", 50 }
            }
        };
        return partsTable;
    }

    private static Table CreateMaterialsTable(IEnumerable<MachineRelease> releases) {
        var usedmaterials = releases.First()
                                            .Programs
                                            .Select(p => p.Material)
                                            .GroupBy(m => (m.Name, m.Width, m.Length, m.Thickness, m.IsGrained));
        var materialTableContent = new List<Dictionary<string, string>>();
        foreach (var mat in usedmaterials) {
            double avgYield = mat.Sum(m => m.Yield) / mat.Count();
            materialTableContent.Add(new() {
                    { "Qty", mat.Count().ToString() },
                    { "Name", mat.Key.Name },
                    { "Width", mat.Key.Width.ToString("0.00") },
                    { "Length", mat.Key.Length.ToString("0.00") },
                    { "Thickness", mat.Key.Thickness.ToString("0.00") },
                    { "Avg. Yield", avgYield.ToString("P2") }
                });
        }

        var materialTable = new Table() {
            Title = "Materials Used",
            Content = materialTableContent,
            ColumnWidths = new Dictionary<string, float> {
                { "Qty", 20 },
                { "Width", 40 },
                { "Length", 40 },
                { "Thickness", 40 },
                { "Avg. Yield", 40 }
            }
        };
        return materialTable;
    }

    private static Table CreateProgramsTable(IEnumerable<MachineRelease> releases) {
        
        var programTableContent = new List<Dictionary<string, string>>();
        int totalPartCount = 0;
        releases.First().Programs.ForEach((program, i) => {
            var programData = new Dictionary<string, string>() {
                { "#", (i + 1).ToString() }
            };

            foreach (var release in releases) {
                string programName = release.Programs.ElementAt(i).Name;
                if (program.HasFace6) {
                    programName += $"\n6{programName[1..]}";
                }
                programData.Add(release.MachineName, programName);
            }

            int sheetPartCount = program.Parts.Count;
            totalPartCount += sheetPartCount;

            programData.Add("Material", $"{program.Material.Name} - {program.Material.Width}x{program.Material.Length}x{program.Material.Thickness}");
            programData.Add("Yield", program.Material.Yield.ToString("P2"));
            programData.Add("Qty", sheetPartCount.ToString());
            programTableContent.Add(programData);

        });

        var columnWidths = new Dictionary<string, float>() {
            { "#", 15 },
            { "Yield", 30 },
            { "Qty", 15 },
        };

        releases.ForEach(release => columnWidths.Add(release.MachineName, 75));

        var programsTable = new Table() {
            Title = $"Nest Programs ({totalPartCount} nested parts)",
            Content = programTableContent,
            ColumnWidths = columnWidths,
        };
        return programsTable;
    }

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

    private static string GetGuidAsBase64(Guid id) => Convert.ToBase64String(id.ToByteArray()).Replace("/", "-").Replace("+", "_").Replace("=", "");

}

