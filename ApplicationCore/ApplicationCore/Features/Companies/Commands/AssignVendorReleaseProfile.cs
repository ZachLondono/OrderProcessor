using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Companies.Commands;

public class AssignVendorReleaseProfile {

    public record Command(Guid VendorId, ReleaseProfile Profile) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command request) {

            using var connection = _factory.CreateConnection();

            const string existQuery = "SELECT 1 FROM releaseprofiles WHERE vendorid = @VendorId;";

            int exists = connection.QuerySingleOrDefault<int>(existQuery, request);

            string command = "";

            if (exists == 1) {

                command = @"UPDATE releaseprofiles
                            SET
                                generatecutlist = @GenerateCutList, cutlistoutputdirectory = @CutListOutputDirectory, printcutlist = @PrintCutList, cutlisttemplatepath = @CutListTemplatePath,
                                generatepackinglist = @GeneratePackingList, packinglistoutputdirectory = @PackingListOutputDirectory, printpackinglist = @PrintPackingList, packinglisttemplatepath = @PackingListTemplatePath,
                                generateinvoice = @GenerateInvoice, invoiceoutputdirectory = @InvoiceOutputDirectory, printinvoice = @PrintInvoice, invoicetemplatepath = @InvoiceTemplatePath,
                                generatebol = @GenerateBOL, boloutputdirectory = @BOLOutputDirectory, printbol = @PrintBOL, boltemplatefilepath = @BOLTemplateFilePath,
                                printboxlabels = @PrintBoxLabels, boxlabelstemplatefilepath = @BoxLabelsTemplateFilePath,
                                printorderlabel = @PrintOrderLabel, orderlabeltemplatefilepath = @OrderLabelTemplateFilePath,
                                printaduiepylelabel = @PrintADuiePyleLabel, aduiepylelabeltemplatefilepath = @ADuiePyleLabelTemplateFilePath,
                                generatecncprograms = @GenerateCNCPrograms, cncreportoutputdirectory = @CNCReportOutputDirectory,
                                filldoororder = @FillDoorOrder, generatedoorprograms = @GenerateDoorCNCPrograms, doororderoutputdirectory = @DoorOrderOutputDirectory, doorordertemplatefilepath = @DoorOrderTemplateFilePath
                            WHERE vendorid = @VendorId;";

            } else {

                command = @"INSERT INTO releaseprofiles
                                (vendorid, generatecutlist, cutlistoutputdirectory, printcutlist, cutlisttemplatepath, generatepackinglist, packinglistoutputdirectory, printpackinglist, packinglisttemplatepath, generateinvoice, invoiceoutputdirectory, printinvoice, invoicetemplatepath, generatebol, boloutputdirectory, printbol, boltemplatefilepath, printboxlabels, boxlabelstemplatefilepath, printorderlabel, orderlabeltemplatefilepath, printaduiepylelabel, aduiepylelabeltemplatefilepath, generatecncprograms, cncreportoutputdirectory, filldoororder, generatedoorprograms, doororderoutputdirectory, doorordertemplatefilepath)
                            VALUES
                                (@VendorId, @GenerateCutList, @CutListOutputDirectory, @PrintCutList, @CutListTemplatePath, @GeneratePackingList, @PackingListOutputDirectory, @PrintPackingList, @PackingListTemplatePath, @GenerateInvoice, @InvoiceOutputDirectory, @PrintInvoice, @InvoiceTemplatePath, @GenerateBOL, @BOLOutputDirectory, @PrintBOL, @BOLTemplateFilePath, @PrintBoxLabels, @BoxLabelsTemplateFilePath, @PrintOrderLabel, @OrderLabelTemplateFilePath, @PrintADuiePyleLabel, @ADuiePyleLabelTemplateFilePath, @GenerateCNCPrograms, @CNCReportOutputDirectory, @FillDoorOrder, @GenerateDoorCNCPrograms, @DoorOrderOutputDirectory, @DoorOrderTemplateFilePath);";

            }

            await connection.ExecuteAsync(command, new {
                request.VendorId,
                request.Profile.GenerateCutList,
                request.Profile.CutListOutputDirectory,
                request.Profile.PrintCutList,
                request.Profile.CutListTemplatePath,
                request.Profile.GeneratePackingList,
                request.Profile.PackingListOutputDirectory,
                request.Profile.PrintPackingList,
                request.Profile.PackingListTemplatePath,
                request.Profile.GenerateInvoice,
                request.Profile.InvoiceOutputDirectory,
                request.Profile.PrintInvoice,
                request.Profile.InvoiceTemplatePath,
                request.Profile.GenerateBOL,
                request.Profile.BOLOutputDirectory,
                request.Profile.PrintBOL,
                request.Profile.BOLTemplateFilePath,
                request.Profile.PrintBoxLabels,
                request.Profile.BoxLabelsTemplateFilePath,
                request.Profile.PrintOrderLabel,
                request.Profile.OrderLabelTemplateFilePath,
                request.Profile.PrintADuiePyleLabel,
                request.Profile.ADuiePyleLabelTemplateFilePath,
                request.Profile.GenerateCNCPrograms,
                request.Profile.CNCReportOutputDirectory,
                request.Profile.FillDoorOrder,
                request.Profile.GenerateDoorCNCPrograms,
                request.Profile.DoorOrderOutputDirectory,
                request.Profile.DoorOrderTemplateFilePath
            });


            return new Response();

        }

    }

}
