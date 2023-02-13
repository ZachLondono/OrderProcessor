using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Infrastructure.Bus;
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
                                generatecutlist = @GenerateCutList, cutlistoutputdirectory = @CutListOutputDirectory, printcutlist = @PrintCutList,
                                generatepackinglist = @GeneratePackingList, packinglistoutputdirectory = @PackingListOutputDirectory, printpackinglist = @PrintPackingList,
                                generateinvoice = @GenerateInvoice, invoiceoutputdirectory = @InvoiceOutputDirectory, printinvoice = @PrintInvoice,
                                generatebol = @GenerateBOL, boloutputdirectory = @BOLOutputDirectory, printbol = @PrintBOL, boltemplatefilepath = @BOLTemplateFilePath,
                                printboxlabels = @PrintBoxLabels, boxlabelstemplatefilepath = @BoxLabelsTemplateFilePath,
                                printorderlabel = @PrintOrderLabel, orderlabeltemplatefilepath = @OrderLabelTemplateFilePath,
                                printaduiepylelabel = @PrintADuiePyleLabel, aduiepylelabeltemplatefilepath = @ADuiePyleLabelTemplateFilePath,
                                generatecncprograms = @GenerateCNCPrograms, cncreportoutputdirectory = @CNCReportOutputDirectory,
                                filldoororder = @FillDoorOrder, generatedoorprograms = @GenerateDoorCNCPrograms, doororderoutputdirectory = @DoorOrderOutputDirectory, doorordertemplatefilepath = @DoorOrderTemplateFilePath,
                                generatecabinetjobsummary = @GenerateCabinetJobSummary, cabinetjobsummarytemplatefilepath = @CabinetJobSummaryTemplateFilePath, cabinetjobsummaryoutputdirectory = @CabinetJobSummaryOutputDirectory,
                                generatecabinetpackinglist = @GenerateCabinetPackingList, cabinetpackinglisttemplatefilepath = @CabinetPackingListTemplateFilePath, cabinetpackinglistoutputdirectory = @CabinetPackingListOutputDirectory
                            WHERE vendorid = @VendorId;"
                ;

            } else {

                command = @"INSERT INTO releaseprofiles
                                (vendorid, generatecutlist, cutlistoutputdirectory, printcutlist, generatepackinglist, packinglistoutputdirectory, printpackinglist, generateinvoice, invoiceoutputdirectory, printinvoice, generatebol, boloutputdirectory, printbol, boltemplatefilepath, printboxlabels, boxlabelstemplatefilepath, printorderlabel, orderlabeltemplatefilepath, printaduiepylelabel, aduiepylelabeltemplatefilepath, generatecncprograms, cncreportoutputdirectory, filldoororder, generatedoorprograms, doororderoutputdirectory, doorordertemplatefilepath, generatecabinetjobsummary, cabinetjobsummarytemplatefilepath, cabinetjobsummaryoutputdirectory, generatecabinetpackinglist, cabinetpackinglisttemplatefilepath, cabinetpackinglistoutputdirectory)
                                VALUES
                                (@VendorId, @GenerateCutList, @CutListOutputDirectory, @PrintCutList, @GeneratePackingList, @PackingListOutputDirectory, @PrintPackingList, @GenerateInvoice, @InvoiceOutputDirectory, @PrintInvoice, @GenerateBOL, @BOLOutputDirectory, @PrintBOL, @BOLTemplateFilePath, @PrintBoxLabels, @BoxLabelsTemplateFilePath, @PrintOrderLabel, @OrderLabelTemplateFilePath, @PrintADuiePyleLabel, @ADuiePyleLabelTemplateFilePath, @GenerateCNCPrograms, @CNCReportOutputDirectory, @FillDoorOrder, @GenerateDoorCNCPrograms, @DoorOrderOutputDirectory, @DoorOrderTemplateFilePath, GenerateCabinetJobSummary, CabinetJobSummaryTemplateFilePath, CabinetJobSummaryOutputDirectory, @GenerateCabinetPackingList, @CabinetPackingListTemplateFilePath, @CabinetPackingListOutputDirectory);";

            }

            await connection.ExecuteAsync(command, new {
                request.VendorId,
                request.Profile.GenerateCutList,
                request.Profile.CutListOutputDirectory,
                request.Profile.PrintCutList,
                request.Profile.GeneratePackingList,
                request.Profile.PackingListOutputDirectory,
                request.Profile.PrintPackingList,
                request.Profile.GenerateInvoice,
                request.Profile.InvoiceOutputDirectory,
                request.Profile.PrintInvoice,
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
                request.Profile.DoorOrderTemplateFilePath,
                request.Profile.GenerateCabinetJobSummary,
                request.Profile.CabinetJobSummaryTemplateFilePath,
                request.Profile.CabinetJobSummaryOutputDirectory,
                request.Profile.GenerateCabinetPackingList,
                request.Profile.CabinetPackingListTemplateFilePath,
                request.Profile.CabinetPackingListOutputDirectory
            });


            return new Response();

        }

    }

}
