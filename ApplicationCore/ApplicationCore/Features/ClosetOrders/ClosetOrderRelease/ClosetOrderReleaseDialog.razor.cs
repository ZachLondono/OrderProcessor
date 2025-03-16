using ApplicationCore.Features.ClosetOrders.ClosetOrderSelector;
using ApplicationCore.Features.GetJobCutListDirectory;
using ApplicationCore.Shared.Settings;
using Blazored.Modal;
using Blazored.Modal.Services;
using Domain.Components.ProgressModal;
using Domain.Infrastructure.Bus;
using Domain.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace ApplicationCore.Features.ClosetOrders.ClosetOrderRelease;

public partial class ClosetOrderReleaseDialog {

    [Parameter]
    public ClosetOrder? Order { get; set; }

    [CascadingParameter]
    public IModalService Modal { get; set; } = default!;

    [CascadingParameter]
    public BlazoredModalInstance BlazoredModal { get; set; } = default!;

    [Inject]
    public IBus? Bus { get; set; }

    [Inject]
    public ClosetOrderReleaseActionRunnerFactory? ActionRunnerFactory { get; set; }

    [Inject]
    public IFilePicker? FilePicker { get; set; }

    [Inject]
    public IOptions<ClosetReleaseSettings> Options { get; set; } = default!;

    public ClosetOrderReleaseOptions Model { get; set; } = new();

    private bool _isLoading = true;
    public bool IsLoading {
        get => _isLoading;
        set {
            _isLoading = value;
            StateHasChanged();
        }
    }

    private string? _errorMessage = null;

    protected override async Task OnAfterRenderAsync(bool firstRender) {

        if (!firstRender) return;

        IsLoading = true;

        if (Order is null || Bus is null) {
            _errorMessage = "";
            IsLoading = false;
            return;
        }

        var settings = Options.Value;

        string outputDirectory = settings.CutListOutputDirectory;

        var outputDirResult = await Bus.Send(new GetJobOrderCutListDirectory.Query(Order.OrderFileDirectory, ""));
        outputDirResult.OnSuccess(dir => {
            if (string.IsNullOrWhiteSpace(dir)) return;
            outputDirectory += ";" + dir;
        });


        string emailRecipients = string.Join(';', settings.ReleaseEmailRecipients);
        string dovetailEmailRecipients = string.Join(';', settings.DovetailDBReleaseEmailRecipients);

        string acknowledgmentEmailRecipeints = string.Empty;
        string invoiceEmailRecipeints = string.Join(';', settings.InvoiceEmailRecipients);

        bool includeCover = true;
        bool includePackingList = true;
        bool includeSummary = false;
        if (settings.ReleaseProfilesByCustomer.TryGetValue(Order.Customer, out var customerProfile)) {
            includeCover = customerProfile.IncludeCover;
            includePackingList = customerProfile.IncludePackingList;
            includeSummary = customerProfile.IncludeSummary;
            acknowledgmentEmailRecipeints = string.Join(';', customerProfile.AcknowledgementEmailRecipients);
            if (customerProfile.InvoiceEmailRecipients.Length > 0) {
                invoiceEmailRecipeints = string.Join(';', customerProfile.InvoiceEmailRecipients) + ";" + invoiceEmailRecipeints;
            }
        }

        string seperatePDFDir = GetJobDirectory(Order.OrderFileDirectory);

        Model = new ClosetOrderReleaseOptions() {

            WorkbookFilePath = Order.OrderFile,

            AddExistingWSXMLReport = false, // TODO: try to find wsxml file automatically
            WSXMLReportFilePath = "",

            FileName = $"{Order.OrderNumber} - Closet Cut List",
            OutputDirectory = outputDirectory,

            IncludeCover = includeCover,
            IncludePackingList = includePackingList,
            IncludePartList = false,
            IncludeDBList = Order.ContainsDovetailBoxes,
            IncludeMDFList = Order.ContainsMDFFronts,
            IncludeOthersList = Order.ContainsOther,
            IncludeSummary = includeSummary,

            SendEmail = !string.IsNullOrWhiteSpace(emailRecipients),
            PreviewEmail = false,
            EmailRecipients = emailRecipients,

            InvoicePDF = true,
            InvoiceDirectory = seperatePDFDir,
            SendInvoiceEmail = !string.IsNullOrWhiteSpace(invoiceEmailRecipeints),
            PreviewInvoiceEmail = false,
            InvoiceEmailRecipients = invoiceEmailRecipeints,

            PreviewAcknowledgementEmail = false,
            SendAcknowledgementEmail = !string.IsNullOrWhiteSpace(acknowledgmentEmailRecipeints),
            AcknowledgmentEmailRecipients = acknowledgmentEmailRecipeints,

            SendDovetailReleaseEmail = Order.ContainsDovetailBoxes,
            PreviewDovetailReleaseEmail = false,
            DovetailReleaseEmailRecipients = dovetailEmailRecipients

        };

        IsLoading = false;

        var file = await GetReportFile(Order.OrderNumber);
        Model.WSXMLReportFilePath = file ?? ""; // TODO: allow for multiple files
        Model.AddExistingWSXMLReport = file is not null;

        StateHasChanged();

    }

    private static string GetJobDirectory(string orderFileDirectory) {

        if (!orderFileDirectory.StartsWith(@"R:\Job Scans")) {
            return orderFileDirectory;
        }

        string directory = orderFileDirectory;
        while (true) {

            var dirInfo = new DirectoryInfo(directory);

            if (dirInfo.Parent is null || dirInfo.Parent.Parent is null) {
                break;
            }

            if (Path.GetFileNameWithoutExtension(dirInfo.Parent.Parent.FullName) is string dirName) {

                if (dirName == "Job Scans") {
                    return dirInfo.FullName;
                }

            } else {
                break;
            }

            directory = dirInfo.Parent.FullName;

        }

        return orderFileDirectory;

    }

    private async Task<string?> GetReportFile(string number) {
        return await Task.Run(() => {
            try {

                var files = Directory.GetFiles(Options.Value.WSXMLReportDirectory, $"{number}*.xml");
                return files.OrderByDescending(file => new FileInfo(file).LastWriteTime)
                            .FirstOrDefault();

            } catch {
                return null;
            }
        });
    }

    private void ChooseReportFile()
        => FilePicker!.PickFile(new() {
            Title = "Select CADCode WS Report File",
            InitialDirectory = Options.Value.WSXMLReportDirectory,
            Filter = new("CADCode WS Report", "xml"),
        }, (fileName) => {
            Model.WSXMLReportFilePath = fileName;
            InvokeAsync(StateHasChanged);
        });

    private async Task GenerateClosetOrderRelease() {

        if (ActionRunnerFactory is null || Order is null) return;

        if (!ValidateModel()) {
            StateHasChanged();
            return;
        }

        var actionRunner = ActionRunnerFactory.CreateActionRunner(Order, Model);

        var parameters = new ModalParameters() {
            { "ActionRunner",  actionRunner },
            { "InProgressTitle", "Releasing Order..." },
            { "CompleteTitle", "Release Complete" }
        };

        var options = new ModalOptions() {
            HideHeader = true,
            HideCloseButton = true,
            DisableBackgroundCancel = true,
            Size = ModalSize.Large
        };

        var dialog = Modal.Show<ProgressModal>("Order Release Progress", parameters, options);
        _ = await dialog.Result;

        await BlazoredModal.CloseAsync();

    }

    private bool ValidateModel() {

        _errorMessage = null;

        if (Model.AddExistingWSXMLReport) {

            if (string.IsNullOrWhiteSpace(Model.WSXMLReportFilePath)) {

                _errorMessage = "Select a WSXML report or uncheck the 'Existing GCode' option.";
                return false;

            }

            if (!File.Exists(Model.WSXMLReportFilePath)) {

                _errorMessage = "Selected WSXML file can not be found.";
                return false;

            }

        }

        string[] outputDirs = Model.OutputDirectory.Split(';');
        foreach (var outputDir in outputDirs) {
            if (Directory.Exists(outputDir)) {
                continue;
            }
            _errorMessage = "One or more output directories does not exist or cannot be accessed.";
            return false;
        }

        if (Model.SendEmail && string.IsNullOrWhiteSpace(Model.EmailRecipients)) {

            _errorMessage = "Specify email recipients or uncheck 'Send Email' option.";
            return false;

        }

        if (Model.InvoicePDF) {

            if (!Directory.Exists(Model.InvoiceDirectory)) {
                _errorMessage = "Invoice output directory does not exist or cannot be accessed.";
                return false;
            }

            if (Model.SendInvoiceEmail && string.IsNullOrWhiteSpace(Model.InvoiceEmailRecipients)) {
                _errorMessage = "No invoice email recipients set.";
                return false;
            }

        }

        if (Model.SendAcknowledgementEmail && string.IsNullOrWhiteSpace(Model.AcknowledgmentEmailRecipients)) {
            _errorMessage = "No acknowledgement email recipients set.";
            return false;
        }

        if (Model.SendDovetailReleaseEmail && string.IsNullOrWhiteSpace(Model.DovetailReleaseEmailRecipients)) {
            _errorMessage = "No dovetail release email recipients set.";
            return false;
        }

        return true;

    }

}