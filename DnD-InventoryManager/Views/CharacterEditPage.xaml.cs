using DnD_InventoryManager.ViewModels;

namespace DnD_InventoryManager.Views;

public partial class CharacterEditPage : ContentPage
{
    public CharacterEditPage(CharacterEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}