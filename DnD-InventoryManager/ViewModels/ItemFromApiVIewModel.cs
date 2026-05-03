using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Api;
using DnD_InventoryManager.Facades;
using DnD_InventoryManager.Models;

namespace DnD_InventoryManager.ViewModels;

[QueryProperty(nameof(Item), "Item")]
public partial class ItemFromApiVIewModel(
    ItemFacade itemFacade
    ) : ViewModelBase
{
    public static IList<ItemCategoriesEnum> AllCategories =>
        Enum.GetValues<ItemCategoriesEnum>().ToList();

    private int _characterId;
    
    [ObservableProperty] 
    public partial ItemModel Item { get; set; } = new();

    [ObservableProperty] 
    public partial string Index { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial ItemCategoriesEnum ItemCategory { get; set; }

    public void LoadPage()
    {
        _characterId = Item.CharacterId;
    }
    
    [RelayCommand]
    private async Task GetItemFromApiAsync()
    {
        try
        {
            Item = ItemCategory switch
            {
                ItemCategoriesEnum.Equipment => await itemFacade.GetFromEquipmentApi(Index),
                ItemCategoriesEnum.MagicItem => await itemFacade.GetFromMagicItemApi(Index),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (ApiException e)
        {
            Console.WriteLine(e);
            await Shell.Current.DisplayAlertAsync("Error", e.Message, "OK");
        }
        
        Item.CharacterId = _characterId;
    }

    [RelayCommand]
    private async Task UseThisItem()
    {
        await Shell.Current.GoToAsync("..", new Dictionary<string, object>()
        {
            {"Item", Item}
        });
    }
}