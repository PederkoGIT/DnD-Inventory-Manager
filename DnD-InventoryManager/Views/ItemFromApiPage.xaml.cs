using DnD_InventoryManager.ViewModels;

namespace DnD_InventoryManager.Views;

public partial class ItemFromApiPage : ContentPage
{
    public ItemFromApiPage(ItemFromApiVIewModel vIewModel)
    {
        InitializeComponent();
        BindingContext = vIewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ItemFromApiVIewModel viewModel)
        {
            await viewModel.LoadPage();
        }
    }

    private void OnSearchedItemChanged(object? sender, TextChangedEventArgs e)
    {
        if (BindingContext is ItemFromApiVIewModel viewModel)
        {
            viewModel.OnSearchedItemChanged();
        }
    }

    private void Picker_OnSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (BindingContext is ItemFromApiVIewModel viewModel)
        {
            viewModel.OnSearchedItemChanged();
        }
    }
}