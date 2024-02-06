using Domain.Companies.Entities;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Features.Companies.Vendors.Queries;
using ApplicationCore.Features.Companies.Vendors.Commands;

namespace ApplicationCore.Features.Companies.Vendors.Edit;

internal class EditVendorViewModel {

    public Action? OnPropertyChanged { get; set; }

    private bool _isLoading = false;
    public bool IsLoading {
        get => _isLoading;
        set {
            _isLoading = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private bool _isSaving = false;
    public bool IsSaving {
        get => _isSaving;
        set {
            _isSaving = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private Error? _error = null;
    public Error? Error {
        get => _error;
        set {
            _error = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public ExistingVendorModel? Model { get; private set; } = null;
    private readonly IBus _bus;

    public EditVendorViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task SetVendorId(Guid vendorId) {

        Error = null;
        IsLoading = true;

        var response = await _bus.Send(new GetVendorById.Query(vendorId));

        Model = response.Match<ExistingVendorModel?>(

            vendor => vendor is null ? null : new() {
                Id = vendorId,
                Name = vendor.Name,
                Phone = vendor.Phone,
                Logo = vendor.Logo,
                LogoFile = string.Empty,
                Address = vendor.Address,
                ExportProfile = vendor.ExportProfile,
                ReleaseProfile = vendor.ReleaseProfile
            },
            error => null
        );

        IsLoading = false;

    }

    public async Task Submit() {

        if (Model is null) {
            Error = new() {
                Title = "Cannot Save",
                Details = "Vendor is not set"
            };
            return;
        }

        Error = null;
        IsSaving = true;

        byte[] logo = Model.Logo;
        try {

            if (File.Exists(Model.LogoFile)) {
                logo = File.ReadAllBytes(Model.LogoFile);
                Model.Logo = logo;
            }

        } catch (Exception ex) {

            _error = new() {
                Title = "Could not read logo file",
                Details = $"{ex.Message}<br><br>{ex.StackTrace}"
            };

        }

        try {

            var vendor = new Vendor(Model.Id, Model.Name ?? "", Model.Address, Model.Phone, logo, Model.ExportProfile, Model.ReleaseProfile);

            var response = await _bus.Send(new UpdateVendor.Command(vendor));

            response.OnError(error => Error = error);

        } catch (Exception ex) {

            _error = new() {
                Title = $"Exception occurred while trying to update vendor in database",
                Details = $"{ex.Message}<br><br>{ex.StackTrace}"
            };

        }

        IsSaving = false;

    }

}
