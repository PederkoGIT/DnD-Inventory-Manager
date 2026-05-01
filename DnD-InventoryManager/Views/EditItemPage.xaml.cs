using DnD_InventoryManager.ViewModels;

namespace DnD_InventoryManager.Views;

public partial class EditItemPage : ContentPage
{
    public EditItemPage(EditItemViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is EditItemViewModel viewModel)
        {
            await viewModel.LoadDataAsync();
        }
    }
}