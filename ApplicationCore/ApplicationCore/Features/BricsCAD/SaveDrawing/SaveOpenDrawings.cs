using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Services;

namespace ApplicationCore.Features.BricsCAD.SaveDrawing;

internal class SaveOpenDrawings {

    public record SaveItem(string DocumentName, string FileName);

    public record Command(IEnumerable<SaveItem> Items) : ICommand;

    public class Handler : CommandHandler<Command> {

        public override Task<Response> Handle(Command command) {

            dynamic? app;

            try {

                app = BricsCADApplicationRetriever.GetAcadApplication();

            } catch (Exception ex) {

                return Task.FromResult(Response.Error(new Error() {
                    Title = "Failed to Access BricsCAD Application",
                    Details = ex.Message
                }));

            }

            if (app is null) {

                return Task.FromResult(Response.Error(new Error() {
                    Title = "Failed to Access BricsCAD Application",
                    Details = "BricsCAD may not be running"
                }));

            }

            List<string> unsavedDocuments = new();

            dynamic? allDocuments;
            
            try {

                allDocuments = app.Documents;

            } catch (Exception ex) {

                return Task.FromResult(Response.Error(new Error() {
                    Title = "Failed to Access BricsCAD Documents",
                    Details = ex.Message
                }));

            }

            foreach (var item in command.Items) {

                bool wasSaved = false;

                try {

                    foreach (dynamic? document in allDocuments) {

                        if (document is null) continue;

                        if (document.Name != item.DocumentName) continue;

                        document.SaveAs(item.FileName);
                        wasSaved = true;
                        
                    }

                } catch (Exception ex) {

                    // TODO: log exception

                }

                if (!wasSaved) unsavedDocuments.Add(item.DocumentName);

            }

            if (unsavedDocuments.Any()) {

                return Task.FromResult(Response.Error(new Error() {
                    Title = "Some or All Documents Could Not Be Saved",
                    Details = $"The following documents where not saved: {string.Join(", ", unsavedDocuments)}"
                }));

            }
            
            return Task.FromResult(Response.Success());

        }

    }

}
