namespace ApplicationCore.Pages.OrderDetails;

public partial class OrderDetailsPage {

    protected override void OnInitialized() {
        DataContext.OnPropertyChanged += StateHasChanged;
    }

}
