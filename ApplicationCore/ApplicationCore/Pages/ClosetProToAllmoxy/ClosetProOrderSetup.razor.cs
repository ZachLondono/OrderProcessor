using ApplicationCore.Features.AllmoxyOrderExport;
using ApplicationCore.Features.ClosetProCSVCutList;
using ApplicationCore.Features.ClosetProToAllmoxyOrder.Models;
using ApplicationCore.Shared.Services;

namespace ApplicationCore.Pages.ClosetProToAllmoxy;

public partial class ClosetProOrderSetup {

    private MappingSettings _settings = new();
    private bool _isLoading = false;
    private bool _isComplete = false;
    private string _error = "";

    public void PickFile() {

        Reset();

        FilePicker.PickFile(new() {
            InitialDirectory = @"C:\Users\Zachary Londono\Downloads",
            Filter = new FilePickerFilter("Closet Pro Cut List", ".csv"),
            Title = "Choose Closet Pro Cut List"
        }, async file => {

            _isLoading = true;
            StateHasChanged();

            try {
                await MapToAllmoxyOrder(file);
                _isComplete = true;
            } catch (Exception ex) {
                _error = ex.Message;
                throw;
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

    public async Task MapToAllmoxyOrder(string fileName) {

        using var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var textReader = new StreamReader(fileStream);
        var csvData = textReader.ReadToEnd();

        var order = await Reader.ReadCSVData(csvData);

        ClosetProPartMapper.MapPickListToItems(order.PickList, [], out var hardwareSpread);
        var cpProducts = CPPartMapper.MapPartsToProducts(order.Parts, hardwareSpread);

        var products = CPToAllmoxyMapper.Map(cpProducts, _settings);

        await CSVOrderWriter.WriteCSVOrder(products, @"C:\Users\Zachary Londono\Desktop\TestOutput\Allmoxy Output\order.csv");

    }

}
