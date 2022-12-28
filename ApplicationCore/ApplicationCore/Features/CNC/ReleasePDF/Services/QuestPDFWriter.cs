using ApplicationCore.Features.CNC.ReleasePDF.Configuration;
using ApplicationCore.Features.CNC.ReleasePDF.Contracts;
using ApplicationCore.Features.CNC.ReleasePDF.PDFModels;
using ApplicationCore.Features.CNC.ReleasePDF.Styling;
using QuestPDF.Fluent;

namespace ApplicationCore.Features.CNC.ReleasePDF.Services;

public class QuestPDFWriter : IReleasePDFWriter {

    private readonly IPDFConfigurationProvider _configProvider;

    public QuestPDFWriter(IPDFConfigurationProvider configProvider) {
        _configProvider = configProvider;
    }

    public IEnumerable<string> GeneratePDFs(ReleasedJob job, string outputDirectory) {

        var createdFiles = new List<string>();

        // TODO: validate configuration
        var config = _configProvider.GetConfiguration();

        foreach (var release in job.Releases)
        {

            CoverModel cover = CreateCover(job, release);
            List<PageModel> pages = CreatePages(job, release);

            var pdfmanager = new PDFBuilder(config) {
                Pages = pages,
                Cover = cover
            };

            try
            {
                var document = pdfmanager.BuildDocument();
                // TODO: get path from configuration
                var filepath = GetFileName(outputDirectory, $"{job.JobName} - {release.MachineName} CUTLIST");
                document.GeneratePdf(filepath);
                createdFiles.Add(filepath);

                //document.WithMetadata(new() {
                //    RasterDpi = 216 // increase resolution of image for printing
                //});
                //var images = document.GenerateImages();
                //foreach (var data in images) {
                //    using var ms = new MemoryStream(data);
                //    var image = System.Drawing.Image.FromStream(ms);
                //    var printer = new Printer(image);
                //    // TODO get printer name (create a printer service which has the printer name configured and used everywhere that a pdf needs to be printed)
                //    //printer.Print("HP4D193E (HP Officejet Pro 8600)");
                //    printer.Print("Microsoft Print to PDF");
                //}

            }
            catch (Exception ex)
            {
                // TODO: warn about failed pdf generation
                Console.WriteLine("Failed to create pdf");
                Console.WriteLine(ex.ToString());
            }

        }

        return createdFiles;

    }

    private static List<PageModel> CreatePages(ReleasedJob job, MachineRelease release) {

        var pages = new List<PageModel>();
        foreach (var program in release.Programs) {

            var partsTableContent = new List<Dictionary<string, string>>();
            var partGroups = program.Parts.GroupBy(p => p.Name);
            foreach (var group in partGroups) {
                var part = group.First();
                partsTableContent.Add(new()  {
                        { "Qty", group.Count().ToString() },
                        { "Name", part.Name },
                        { "Width", part.Width.ToString() },
                        { "Length", part.Length.ToString() },
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
                Subtitle = $"{material.Name} - {material.Width}x{material.Length}x{material.Thickness} (grained:{(material.IsGrained ? "yes" : "no")})",

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
                    { "Width", mat.Key.Width.ToString() },
                    { "Length", mat.Key.Length.ToString() },
                    { "Thickness", mat.Key.Thickness.ToString() },
                    { "Grained", mat.Key.IsGrained ? "yes" : "no" },
                });
        }

        var materialTable = new Table() {
            Title = "Materials Used",
            Content = materialTableContent
        };

        var releasedparts = release.Programs.SelectMany(p => p.Parts).GroupBy(p => p.Name);
        var partsTableContent = new List<Dictionary<string, string>>();
        foreach (var group in releasedparts) {
            var part = group.First();
            partsTableContent.Add(new() {
                    { "Qty", group.Count().ToString() },
                    { "Name", part.Name },
                    { "Width", part.Width.ToString() },
                    { "Length", part.Length.ToString() },
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

        var cover = new CoverModel() {
            Title = $"{job.JobName}  [{release.MachineName}]",
            Info = new Dictionary<string, string>() {
                    {"Vendor", "VendorName" },
                    {"Customer", "CustomerName" },
                    {"Order Date", DateTime.Today.ToShortDateString() },
                    {"Due Date", DateTime.Today.ToShortDateString() }
                },
            Tables = tables
        };
        return cover;
    }

    private static string GetFileName(string path, string filename) {

        int num = 0;

        string fullpath = Path.Combine(path, $"{filename}.pdf");
        while (File.Exists(fullpath)) {
            fullpath = Path.Combine(path, $"{filename} ({++num}).pdf");
        }

        return fullpath;

    }

}
