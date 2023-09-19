using ApplicationCore.Features.Orders.Shared.Domain.Products.Closets;
using ApplicationCore.Features.Products.UpdateClosetPart;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Widgets.Products.ClosetPartEditor;

public partial class ClosetPartEditor {

	[Parameter]
	public ClosetPart? Product { get; set; }

    [CascadingParameter]
    private BlazoredModalInstance BlazoredModal { get; set; } = default!;

	[Inject]
	public IBus? Bus { get; set; }

	[Inject]
	public IModalService? ModalService { get; set; }

	public async Task Update() {

		if (Bus is null || Product is null) return;

		var response = await Bus.Send(new UpdateClosetPart.Command(Product));

		await response.Match(
			unit => BlazoredModal.CloseAsync(),
			error => {
				if (ModalService is not null) {
					return ModalService.OpenErrorDialog(error);
				}
				return BlazoredModal.CloseAsync();
			});

	}

	public Task Cancel() => BlazoredModal.CloseAsync();

}
