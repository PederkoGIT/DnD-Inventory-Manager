using DnD_InventoryManager.Views;

namespace DnD_InventoryManager;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(CharacterEditPage), typeof(CharacterEditPage));
        Routing.RegisterRoute(nameof(CharacterDetailPage), typeof(CharacterDetailPage));
        Routing.RegisterRoute(nameof(DiceRollerPage), typeof(DiceRollerPage));
        Routing.RegisterRoute(nameof(QrCodeDisplayPage), typeof(QrCodeDisplayPage));
        Routing.RegisterRoute(nameof(QrScanPage), typeof(QrScanPage));

        Routing.RegisterRoute(nameof(ItemEditPage), typeof(ItemEditPage));
        Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
        Routing.RegisterRoute(nameof(ItemFromApiPage), typeof(ItemFromApiPage));
    }
}