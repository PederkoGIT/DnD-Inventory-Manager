using DnD_InventoryManager.ViewModels;

namespace DnD_InventoryManager.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();

            if (BindingContext is MainViewModel vm)
            {
                await vm.LoadCharactersAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is not MainViewModel { IsWaitingForNfc: true } viewModel)
            return base.OnBackButtonPressed();
        viewModel.CancelNfcCommand.Execute(null);
            
        return true;

    }
}