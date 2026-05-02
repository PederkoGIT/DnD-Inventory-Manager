using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Facades;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Views;

namespace DnD_InventoryManager.ViewModels;

[QueryProperty("Item", nameof(Item))]
public partial class ItemDetailViewModel(
    ItemFacade itemFacade
    ) : ViewModelBase
{
    [ObservableProperty] public partial ItemModel? Item { get; set; } = new();

    public async Task RefreshItemAsync()
    {
        if (Item == null || Item.Id == 0)
        {
            return;
        }

        var itemFormDb = await itemFacade.GetByIdAsync(Item.Id);
        if (itemFormDb != null)
        {
            Item = itemFormDb;
            Title = Item.Name;    
        }
    }

    [RelayCommand]
    private async Task GoToEditItemAsync()
    {
        if (Item == null)
        {
            return;
        }

        await Shell.Current.GoToAsync(nameof(ItemEditPage), new Dictionary<string, object>
        {
            { nameof(Item), Item }

        });
    }

    [RelayCommand]
    private async Task DeleteItemAsync()
    {
        if (Item == null || Item.Id == 0)
        {
            return;
        }
        
        var answer = await Shell.Current.DisplayAlertAsync("Delete Character", "Are you sure you want to delete this item?", "Yes", "No");
        if (answer)
        {
            await itemFacade.DeleteAsync(Item.Id);
            await Shell.Current.GoToAsync("..");
        }
    }
}