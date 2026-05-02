using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Facades;
using DnD_InventoryManager.Models;

namespace DnD_InventoryManager.ViewModels;

[QueryProperty(nameof(Item), "Item")]
public partial class ItemEditViewModel(
    ItemFacade itemFacade
    ) : ViewModelBase
{
    [ObservableProperty] public partial Item Item { get; set; } = new() ;

    public async Task LoadDataAsync()
    {
        Title = "Add Item";
        var itemFromDb = await itemFacade.GetByIdAsync(Item.Id);
        if (itemFromDb is not null)
        {
            Title = "Edit Item";
            Item = itemFromDb;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await itemFacade.SaveAsync(Item);
        await Shell.Current.GoToAsync("..");
    }
}