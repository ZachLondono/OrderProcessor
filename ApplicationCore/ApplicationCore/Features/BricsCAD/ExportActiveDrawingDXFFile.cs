using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Services;
using BricscadApp;

namespace ApplicationCore.Features.BricsCAD;

public class ExportActiveDrawingDXFFile {

    public record Command(string ExportFileName) : ICommand;

    public class Handler : CommandHandler<Command> {

        public override async Task<Response> Handle(Command command) {

            AcadApplication? app = null;

            try {
                app = BricsCADApplicationRetriever.GetAcadApplication();
            } catch { }

            if (app is null) {
                return new Error() {
                    Title = "BricsCAD Not Found",
                    Details = "Failed to get instance of BricsCAD application."
                };
            }

            AcadDocument? document = null;

            try {
                document = app.ActiveDocument;
            } catch { }

            if (document is null) {
                return new Error() {
                    Title = "No Document Found",
                    Details = "Failed to get active BricsCAD document."
                };
            }

            try {

                await Task.Run(() => document.Export(command.ExportFileName, "dxf", document.ActiveSelectionSet));

            } catch {
                return new Error() {
                    Title = "Could Not Export DXF",
                    Details = "Error occurred while trying to export dxf from BricsCAD"
                };
            }

            return Response.Success();

        }

    }

}
