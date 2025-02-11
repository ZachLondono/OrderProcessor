using ApplicationCore.Shared.Services;
using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Microsoft.Office.Interop.Outlook;

namespace ApplicationCore.Features.MDFDoorOrders.HafeleMDFDoorOrders;

public partial class ImportHafeleOrder {

    private bool _isLoading = true;
    private string? _loadingError = null;
    private EmailDetails? _emailDetails = null;
    private MailItem? _mailItem = null;
    private OutlookWrapper? _wrapper = null;
    private State _state = State.Setup;

    [CascadingParameter]
    private BlazoredModalInstance ModalInstance { get; set; } = default!;

    [Inject]
    private HafeleMDFDoorImporter Importer { get; set; } = default!;

    protected override void OnAfterRender(bool firstRender) {

        if (!firstRender) return;

        LoadCurrentMessageDetails();

    }

    private void LoadCurrentMessageDetails() {

        _isLoading = true;
        _loadingError = null;
        _emailDetails = null;
        _mailItem = null;
        _state = State.Setup;
        StateHasChanged();

        try {

            if (_wrapper is null) _wrapper = new OutlookWrapper();
            if (!_wrapper.IsInitialized) _wrapper.Initialize();
            _mailItem = _wrapper.GetCurrentlySelectedMailItem();

            if (_mailItem is null) {
                _isLoading = false;
                StateHasChanged();
                return;
            }

            _emailDetails = GetMessageDetails(_mailItem);

            _isLoading = false;
            StateHasChanged();

        } catch {

            _isLoading = false;
            _loadingError = "Couldn't get selected email";
            StateHasChanged();

        }

    }

    private static EmailDetails GetMessageDetails(MailItem mailItem) {

        List<EmailAttachment> attachments = [];
        foreach (Attachment attachment in mailItem.Attachments) {
            var ext = Path.GetExtension(attachment.FileName);
            bool incoming = true;
            bool orders = ext.Equals(".xlsx");
            attachments.Add(new(attachment.Index, attachment.FileName, incoming, orders));
        }

        return new() {
            Company = "",
            OrderNumber = "",
            Attachments = attachments.ToArray()
        };

    }

    private async Task ImportSelectedOrder() {

        if (_emailDetails is null || _mailItem is null) {
            _loadingError = "No email selected";
            return;
        }

        _state = State.Importing;
        StateHasChanged();

        await Task.Run(() => {
            Importer.ImportOrderFromMailItem(_emailDetails, _mailItem);
        });

        _state = State.Complete;
        StateHasChanged();

    }

    private async Task Cancel() {
        await ModalInstance.CancelAsync();
    }

    enum State {
        Setup,
        Importing,
        Complete
    }

}
