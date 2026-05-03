using DnD_InventoryManager.ViewModels;

namespace DnD_InventoryManager.Views;

public partial class ItemDetailPage : ContentPage
{
    public ItemDetailPage(ItemDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();

            if (BindingContext is ItemDetailViewModel viewModel)
            {
                await viewModel.RefreshItemAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}