using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Pages.OrderList;

public partial class OrderListPage {

    public void OnSearchTermChange(ChangeEventArgs args) {

        if (args.Value is string val) {
            DataContext.SearchTerm = val;
        } else {
            DataContext.SearchTerm = null;
        }

    }

    protected override async Task OnInitializedAsync() {

        await DataContext.LoadCompanies();

    }


}
