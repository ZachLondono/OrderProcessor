using ApplicationCore.Features.AllmoxyOrderExport;
using ApplicationCore.Features.AllmoxyOrderExport.Products;
using ApplicationCore.Features.ClosetProToAllmoxyOrder.Models;
using ApplicationCore.Shared.Services;
using Domain.Services;
using OrderLoading.ClosetProCSVCutList;
using OrderLoading.ClosetProCSVCutList.Products;
using System.Diagnostics;

namespace ApplicationCore.Pages.ClosetProToAllmoxy;

public partial class ClosetProOrderSetup {

    private readonly CPToAllmoxyMappingSettings _mappingSettings = new();
    private readonly ClosetProLoadingSettings _loadingSettings = new();
    private bool _isLoading = false;
    private bool _isFileSelected = false;
    private bool _isComplete = false;
    private string _allmoxyOutputFile = @"C:\Users\Zachary Londono\Desktop\TestOutput\Allmoxy Output\order.csv";
    private string _error = "";

    public IEnumerable<IClosetProProduct> ClosetProProducts { get; set; } = [];
    public IEnumerable<IAllmoxyProduct> AllmoxyProducts { get; set; } = [];

    public void PickFile() {

        Reset();

        var initialDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads"
        );

        FilePicker.PickFile(new() {
            InitialDirectory = initialDir,
            Filter = new FilePickerFilter("Closet Pro Cut List", ".csv"),
            Title = "Choose Closet Pro Cut List"
        }, async file => {

            _isLoading = true;
            _isFileSelected = true;
            StateHasChanged();

            try {
                await LoadClosetProProducts(file);
                _isComplete = true;
                _error = "";
            } catch (Exception ex) {
                _error = ex.Message;
                _isFileSelected = false;
            } finally {
                Reset();
                StateHasChanged();
            }


        });

    }

    private void Reset() {
        _isComplete = false;
        _isLoading = false;
    }

    public async Task LoadClosetProProducts(string fileName) {

        using var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var textReader = new StreamReader(fileStream);
        var csvData = textReader.ReadToEnd();

        var order = await Reader.ReadCSVData(csvData);

        ClosetProPartMapper.MapPickListToItems(order.PickList, [], out var hardwareSpread);
        CPPartMapper.GroupLikeProducts = _loadingSettings.GroupLikeProducts;
        CPPartMapper.RoomNamingStrategy = _loadingSettings.RoomNamingStrategy;
        ClosetProProducts = CPPartMapper.MapPartsToProducts(order.Parts, hardwareSpread);

    }

    public async Task MapToAllmoxyOrder() {

        if (!ClosetProProducts.Any()) {
            _error = "No products loaded to map";
            return;
        }

        _isLoading = true;

        try {

            AllmoxyProducts = CPToAllmoxyMapper.Map(ClosetProProducts, _mappingSettings);
            await CSVOrderWriter.WriteCSVOrder(AllmoxyProducts, _allmoxyOutputFile);
            _isComplete = true;

        } catch (Exception ex) {

            _error = ex.Message;

        } finally {

            _isLoading = false;
            StateHasChanged();

        }

    }

    public void OpenFile(string filePath) {

        try {

            var psi = new ProcessStartInfo {
                FileName = filePath,
                UseShellExecute = true
            };
            Process.Start(psi);

        } catch (Exception ex) {
            Debug.WriteLine(ex);
        }

    }

}
