using DnD_InventoryManager.ViewModels;

namespace DnD_InventoryManager.Views;

public partial class QrCodeDisplayPage : ContentPage
{
    public QrCodeDisplayPage(QrCodeDisplayViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}