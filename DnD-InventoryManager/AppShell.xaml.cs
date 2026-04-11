using DnD_InventoryManager.Views;

namespace DnD_InventoryManager;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(EditCharacterPage), typeof(EditCharacterPage));
        Routing.RegisterRoute(nameof(CharacterDetailPage), typeof(CharacterDetailPage));
    }
}