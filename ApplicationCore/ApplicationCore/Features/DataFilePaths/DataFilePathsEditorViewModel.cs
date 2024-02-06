using Domain.Infrastructure.Settings;

namespace ApplicationCore.Features.Configuration;

internal class DataFilePathsEditorViewModel {

    public Action? OnPropertyChanged { get; set; }

    private Domain.Infrastructure.Settings.DataFilePaths? _configuration = null;
    public Domain.Infrastructure.Settings.DataFilePaths? Configuration {
        get => _configuration;
        private set {
            _configuration = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private string? _error = null;
    public string? ErrorMessage {
        get => _error;
        set {
            _error = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private readonly IWritableOptions<Domain.Infrastructure.Settings.DataFilePaths> _options;

    public DataFilePathsEditorViewModel(IWritableOptions<Domain.Infrastructure.Settings.DataFilePaths> options) {
        _options = options;

        try {
            _configuration = _options.Value;
            ErrorMessage = null;
        } catch {
            ErrorMessage = "Settings could not be loaded";
        }
    }

    public void SaveChanges() {
        if (Configuration is null) return;

        try {

            _options.Update(option => {
                option.OrderingDBPath = Configuration.OrderingDBPath;
                option.CompaniesDBPath = Configuration.CompaniesDBPath;
            });

            ErrorMessage = null;

        } catch {

            ErrorMessage = "Changes could not be saved";

        }

    }

}
