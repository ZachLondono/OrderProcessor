using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Components;
using Blazored.Modal;
using Blazored.Modal.Services;
using static ApplicationCore.Shared.Components.InformationDialog;

namespace ApplicationCore.Shared;

internal static class ModalExtensions {

    public static Task<ModalResult> OpenInformationDialog(this IModalService modal, string title, string details, MessageType messageType, ModalSize? size = null) {

        var options = new ModalOptions() {
            HideHeader = true,
            Size = size
        };

        var parameters = new ModalParameters {
            { "Type", messageType },
            { "Title", title },
            { "Details", details }
        };

        return modal.Show<InformationDialog>("Information", parameters, options).Result;

    }

    public static Task<ModalResult> OpenErrorDialog(this IModalService modal, Error error, ModalSize? size = null) => modal.OpenInformationDialog(error.Title, error.Details, MessageType.Error, size);
    public static Task<ModalResult> OpenErrorDialog(this IModalService modal, Error error) => modal.OpenInformationDialog(error.Title, error.Details, MessageType.Error, null);

}
