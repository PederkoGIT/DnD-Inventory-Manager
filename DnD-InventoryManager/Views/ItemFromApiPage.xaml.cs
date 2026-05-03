using DnD_InventoryManager.ViewModels;

namespace DnD_InventoryManager.Views;

public partial class ItemFromApiPage : ContentPage
{
    public ItemFromApiPage(ItemFromApiVIewModel vIewModel)
    {
        InitializeComponent();
        BindingContext = vIewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ItemFromApiVIewModel viewModel)
        {
            viewModel.LoadPage();
        }
    }
}