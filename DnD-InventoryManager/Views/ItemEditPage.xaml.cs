using DnD_InventoryManager.ViewModels;

namespace DnD_InventoryManager.Views;

public partial class ItemEditPage : ContentPage
{
    public ItemEditPage(ItemEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ItemEditViewModel viewModel)
        {
            await viewModel.LoadDataAsync();
        }
    }
}