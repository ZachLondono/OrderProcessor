using ApplicationCore.Features.AllmoxyOrderExport;
using ApplicationCore.Features.AllmoxyOrderExport.Products;
using ApplicationCore.Features.ClosetProCSVCutList;
using ApplicationCore.Features.ClosetProCSVCutList.Products;
using ApplicationCore.Features.ClosetProToAllmoxyOrder.Models;
using ApplicationCore.Shared.Services;
using System.Diagnostics;

namespace ApplicationCore.Pages.ClosetProToAllmoxy;

public partial class ClosetProOrderSetup {

    private MappingSettings _settings = new();
    private bool _isLoading = false;
    private bool _isFileSelected = false;
    private bool _isComplete = false;
    private string _allmoxyOutputFile = @"C:\Users\Zachary Londono\Desktop\TestOutput\Allmoxy Output\order.csv";
    private string _error = "";

    public IEnumerable<IClosetProProduct> ClosetProProducts { get; set; } = [];
    public IEnumerable<IAllmoxyProduct> AllmoxyProducts { get; set; } = [];

    public void PickFile() {

        Reset();

        FilePicker.PickFile(new() {
            InitialDirectory = @"C:\Users\Zachary Londono\Downloads",
            Filter = new FilePickerFilter("Closet Pro Cut List", ".csv"),
            Title = "Choose Closet Pro Cut List"
        }, async file => {

            _isLoading = true;
            _isFileSelected = true;
            StateHasChanged();

            try {
                await LoadClosetProProducts(file);
                _isComplete = true;
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
        _error = "";
    }

    public async Task LoadClosetProProducts(string fileName) {

        using var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var textReader = new StreamReader(fileStream);
        var csvData = textReader.ReadToEnd();

        var order = await Reader.ReadCSVData(csvData);

        ClosetProPartMapper.MapPickListToItems(order.PickList, [], out var hardwareSpread);
        ClosetProProducts = CPPartMapper.MapPartsToProducts(order.Parts, hardwareSpread);

    }

    public async Task MapToAllmoxyOrder() {

        if (!ClosetProProducts.Any()) {
            _error = "No products loaded to map";
            return;
        }

        _isLoading = true;

        try {

            AllmoxyProducts = CPToAllmoxyMapper.Map(ClosetProProducts, _settings);
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
