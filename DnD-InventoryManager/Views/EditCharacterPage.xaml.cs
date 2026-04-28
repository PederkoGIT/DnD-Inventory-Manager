using DnD_InventoryManager.ViewModels;

namespace DnD_InventoryManager.Views;

public partial class EditCharacterPage : ContentPage
{
    public EditCharacterPage(EditCharacterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}