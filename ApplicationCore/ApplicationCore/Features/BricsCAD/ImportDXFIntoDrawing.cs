using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Services;
using BricscadApp;

namespace ApplicationCore.Features.BricsCAD;

public class ImportDXFIntoDrawing {

    public record Command(string DXFFilePath, ImportMode Mode) : ICommand;

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

            try {

                await Task.Run(() => {

                    if (command.Mode is ImportMode.Replace) {
                        document.SendCommand("SELECT ALL  ERASE ");
                    }

                    var insertionPoint = new double[] { 0, 0, 0 };
                    var scale = 1.0;
                    document.Import(command.DXFFilePath, insertionPoint, scale);

                });

            } catch {

                return new Error() {
                    Title = "Failed to Insert DXF",
                    Details = "An error occurred while trying to insert DXF data into active document."
                };

            }

            return Response.Success();

        }

    }

}
