using ApplicationCore.Shared.Services;
using BricscadApp;
using Domain.Infrastructure.Bus;

namespace ApplicationCore.Features.Orders.ProductDrawings.Commands;

public class ImportProductDrawingIntoActiveDocument {

    public record Command(byte[] DrawingData, ImportMode Mode) : ICommand;

    public enum ImportMode {
        New,
        Import,
        Replace
    }

    public class Handler : CommandHandler<Command> {

        public override async Task<Response> Handle(Command command) {

            AcadApplication? app = null;

            try {
                app = BricsCADApplicationRetriever.GetAcadApplication(command.Mode == ImportMode.New);
            } catch { }

            if (app is null) {
                return new Error() {
                    Title = "BricsCAD Not Found",
                    Details = "Failed to get instance of BricsCAD application."
                };
            }

            AcadDocument? document = null;

            if (command.Mode == ImportMode.New) {

                document = app.Documents.Add();

                if (document is null) {
                    return new Error() {
                        Title = "Could not Create Document",
                        Details = "Failed to create new BricsCAD document."
                    };
                }

            } else {

                try {
                    document = app.ActiveDocument;
                } catch { }

                if (document is null) {
                    return new Error() {
                        Title = "No Document Found",
                        Details = "Failed to get active BricsCAD document."
                    };
                }

            }

            var uncompressed = CompressionService.Uncompress(command.DrawingData);

            var dxfAscii = System.Text.Encoding.ASCII.GetString(uncompressed);

            var tmpFilePath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".dxf";

            await File.WriteAllTextAsync(tmpFilePath, dxfAscii);

            if (command.Mode is ImportMode.Replace) {
                document.SendCommand("SELECT ALL  ERASE ");
            }

            var insertionPoint = new double[] { 0, 0, 0 };
            var scale = 1.0;
            document.Import(tmpFilePath, insertionPoint, scale);

            File.Delete(tmpFilePath);

            return Response.Success();

        }

    }

}
