using DnD_InventoryManager.ViewModels;

namespace DnD_InventoryManager.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    
}