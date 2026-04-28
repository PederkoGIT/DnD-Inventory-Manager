using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DnD_InventoryManager.ViewModels;

namespace DnD_InventoryManager.Views;

public partial class CharacterDetailPage : ContentPage
{
    public CharacterDetailPage(CharacterDetailViewModel viewModel)
    { 
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is CharacterDetailViewModel viewModel)
        {
            await viewModel.RefreshCharacterAsync();
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is CharacterDetailViewModel viewModel && viewModel.IsWaitingForNfc)
        {
            viewModel.CancelNfcCommand.Execute(null);

            return true;
        }
        
        return base.OnBackButtonPressed();
    }
}