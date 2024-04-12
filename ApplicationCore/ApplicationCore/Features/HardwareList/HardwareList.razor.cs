using ApplicationCore.Features.HardwareList.Commands;
using ApplicationCore.Features.HardwareList.Models;
using ApplicationCore.Features.HardwareList.Queries;
using Domain.Infrastructure.Bus;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Components;
using Windows.System.Update;

namespace ApplicationCore.Features.HardwareList;

public partial class HardwareList {

    [Inject]
    public IBus Bus { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    public Guid OrderId { get; set; }

    private Hardware? _hardware = null;

    private HardwareEditModel? _hardwareList = null;
    public HardwareEditModel? Hardware {
        get => _hardwareList;
        set {
            _hardwareList = value;
            StateHasChanged();
        }
    }

    private Error? _error = null;
    public Error? Error {
        get => _error;
        set {
            _error = value;
            StateHasChanged();
        }
    }

    private bool _isLoading = false;
    public bool IsLoading {
        get => _isLoading;
        set {
            _isLoading = value;
            StateHasChanged();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender) {

        await base.OnAfterRenderAsync(firstRender);
        if (!firstRender) return;

        var result = await Bus.Send(new GetHardwareList.Query(OrderId));

        result.Match(
            hardware => {
                _hardware = hardware;
                Hardware = new() {
                    Supplies = _hardware.Supplies
                                        .Select(s => new SupplyEditModel() {
                                            Id = s.Id,
                                            Qty = s.Qty,
                                            Description = s.Description
                                        })
                                        .ToList(),
                    DrawerSlides = _hardware.DrawerSlides
                                        .Select(s => new DrawerSlideEditModel() {
                                            Id = s.Id,
                                            Qty = s.Qty,
                                            Length = s.Length,
                                            Style = s.Style
                                        })
                                        .ToList(),
                    HangingRails = _hardware.HangingRails
                                        .Select(s => new HangingRailEditModel() {
                                            Id = s.Id,
                                            Qty = s.Qty,
                                            Length = s.Length,
                                            Finish = s.Finish
                                        })
                                        .ToList(),
                };
            },
            error => Error = error);

    }

    private async Task AddSupply() {

        if (Hardware is null) return;

        var supply = new Supply(Guid.NewGuid(), 1, string.Empty);
        var result = await Bus.Send(new AddSupplyToOrder.Command(OrderId, supply));

        result.Match(
            _ => {

                Hardware.Supplies.Add(new() {
                    Id = supply.Id,
                    Qty = supply.Qty,
                    Description = supply.Description
                });

            },
            error => Error = error);

        StateHasChanged();

    }

    private async Task UpdateSupply(SupplyEditModel model) {

        var supply = new Supply(model.Id, model.Qty, model.Description);

        var result = await Bus.Send(new UpdateSupply.Command(supply));

        result.Match(
            _ => model.IsDirty = false,
            error => Error = error
        );

        StateHasChanged();

    }

    private async Task DeleteSupply(SupplyEditModel model) {

        if (Hardware is null) return;

        var result = await Bus.Send(new DeleteSupply.Command(model.Id));

        result.Match(
            _ => Hardware.Supplies.Remove(model),
            error => Error = error
        );

        StateHasChanged();

    }

    private async Task AddSlides() {

        if (Hardware is null) return;

        var slide = new DrawerSlide(Guid.NewGuid(), 1, Dimension.FromInches(21), "Undermount Slides");
        var result = await Bus.Send(new AddDrawerSlideToOrder.Command(OrderId, slide));

        result.Match(
            _ => {

                Hardware.DrawerSlides.Add(new() {
                    Id = slide.Id,
                    Qty = slide.Qty,
                    Length = slide.Length,
                    Style = slide.Style
                });

            },
            error => Error = error);

        StateHasChanged();

    }

    private async Task UpdateDrawerSlide(DrawerSlideEditModel model) {

        var slide = new DrawerSlide(model.Id, model.Qty, model.Length, model.Style);

        var result = await Bus.Send(new UpdateDrawerSlide.Command(slide));

        result.Match(
            _ => model.IsDirty = false,
            error => Error = error
        );

    }

    private async Task DeleteDrawerSlide(DrawerSlideEditModel model) {

        if (Hardware is null) return;

        var result = await Bus.Send(new DeleteDrawerSlide.Command(model.Id));

        result.Match(
            _ => Hardware.DrawerSlides.Remove(model),
            error => Error = error
        );

        StateHasChanged();

    }

    private async Task AddRails() {

        if (Hardware is null) return;

        var rail = new HangingRail(Guid.NewGuid(), 1, Dimension.FromInches(21), "Chrome");
        var result = await Bus.Send(new AddHangingRailToOrder.Command(OrderId, rail));

        result.Match(
            _ => {

                Hardware.HangingRails.Add(new() {
                    Id = rail.Id,
                    Qty = rail.Qty,
                    Length = rail.Length,
                    Finish = rail.Finish
                });

            },
            error => Error = error);

        StateHasChanged();

    }

    private async Task UpdateHangingRail(HangingRailEditModel model) {

        var rail = new HangingRail(model.Id, model.Qty, model.Length, model.Finish);

        var result = await Bus.Send(new UpdateHangingRail.Command(rail));

        result.Match(
            _ => model.IsDirty = false,
            error => Error = error
        );

    }

    private async Task DeleteHangingRail(HangingRailEditModel model) {

        if (Hardware is null) return;

        var result = await Bus.Send(new DeleteHangingRail.Command(model.Id));

        result.Match(
            _ => Hardware.HangingRails.Remove(model),
            error => Error = error
        );

        StateHasChanged();

    }

}
