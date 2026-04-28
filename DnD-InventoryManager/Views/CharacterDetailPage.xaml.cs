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
        try
        {
            base.OnAppearing();

            if (BindingContext is CharacterDetailViewModel viewModel)
            {
                await viewModel.RefreshCharacterAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is not CharacterDetailViewModel { IsWaitingForNfc: true } viewModel)
            return base.OnBackButtonPressed();
        viewModel.CancelNfcCommand.Execute(null);

        return true;

    }
}