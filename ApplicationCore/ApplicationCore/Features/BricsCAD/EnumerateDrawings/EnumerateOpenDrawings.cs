using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Services;

namespace ApplicationCore.Features.BricsCAD.EnumerateDrawings;

public class EnumerateOpenDrawings {

    public record Command() : ICommand<IEnumerable<string>>;

    public class Handler : CommandHandler<Command, IEnumerable<string>> {

        // TODO: check if the document contains a FILENAME layer with text, and return that with the document name. It can be used as the file name when saving the drawing

        public override Task<Response<IEnumerable<string>>> Handle(Command command) {
            
            dynamic? app;

            try {

                app = BricsCADApplicationRetriever.GetAcadApplication();

            } catch (Exception ex) {

                return Task.FromResult(Response<IEnumerable<string>>.Error(new Error() {
                    Title = "Failed to Access BricsCAD Application",
                    Details = ex.Message
                }));

            }

            if (app is null) {

                return Task.FromResult(Response<IEnumerable<string>>.Error(new Error() {
                    Title = "Failed to Access BricsCAD Application",
                    Details = "BricsCAD may not be running"
                }));

            }

            dynamic? allDocuments;
            
            try {

                allDocuments = app.Documents;

            } catch (Exception ex) {

                return Task.FromResult(Response<IEnumerable<string>>.Error(new Error() {
                    Title = "Failed to Access BricsCAD Documents",
                    Details = ex.Message
                }));

            }

            List<string> documentNames = new();

            foreach (dynamic? document in allDocuments) {

                if (document is null) continue;

                documentNames.Add(document.Name);

            }

            return Task.FromResult(new Response<IEnumerable<string>>(documentNames));

        }

    }

}
