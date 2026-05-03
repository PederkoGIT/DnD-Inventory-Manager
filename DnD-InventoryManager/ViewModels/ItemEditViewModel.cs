using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Facades;
using DnD_InventoryManager.Models;

namespace DnD_InventoryManager.ViewModels;

[QueryProperty(nameof(ItemModel), "Item")]
public partial class ItemEditViewModel(
    ItemFacade itemFacade
    ) : ViewModelBase
{
    [ObservableProperty] public partial ItemModel ItemModel { get; set; } = new() ;

    public async Task LoadDataAsync()
    {
        Title = "Add Item";
        var itemFromDb = await itemFacade.GetByIdAsync(ItemModel.Id);
        if (itemFromDb is not null)
        {
            Title = "Edit Item";
            ItemModel = itemFromDb;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await itemFacade.SaveAsync(ItemModel);
        await Shell.Current.GoToAsync("..");
    }
}