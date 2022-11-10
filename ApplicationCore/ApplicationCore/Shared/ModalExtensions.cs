using ApplicationCore.Infrastructure;
using Blazored.Modal;
using Blazored.Modal.Services;
using static ApplicationCore.Shared.InformationDialog;

namespace ApplicationCore.Shared;

internal static class ModalExtensions {

    public static IModalReference OpenInformationDialog(this IModalService modal, string title, string details, MessageType messageType) {

        var options = new ModalOptions() {
            HideHeader = true
        };

        var parameters = new ModalParameters {
            { "Type", messageType },
            { "Title", title },
            { "Details", details }
        };

        return modal.Show<InformationDialog>("Information", parameters, options);

    }

    public static IModalReference OpenErrorDialog(this IModalService modal, Error error) => OpenInformationDialog(modal, error.Title, error.Details, MessageType.Error);

}
