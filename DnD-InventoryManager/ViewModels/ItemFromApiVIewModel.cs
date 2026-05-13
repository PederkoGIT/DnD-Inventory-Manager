using System.Collections.ObjectModel;
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
    private List<ItemListApiModel> _allEquipmentApiList = []; 
    private List<ItemListApiModel> _allMagicItemsApiList = []; 
    
    [ObservableProperty]
    public partial ObservableCollection<ItemListApiModel> EquipmentApiList { get; set; } = [];
    
    [ObservableProperty] 
    public partial string SearchedItem { get; set; } = string.Empty;
    
    [ObservableProperty] 
    public partial ItemModel Item { get; set; } = new();

    [ObservableProperty] 
    public partial ItemListApiModel? SelectedItem { get; set; }
    
    [ObservableProperty]
    public partial ItemCategoriesEnum ItemCategory { get; set; }

    public async Task LoadPage()
    {
        IsBusy = true;
        _characterId = Item.CharacterId;
        
        _allEquipmentApiList = await itemFacade.GetAllEquipmentApiAsync();
        _allMagicItemsApiList = await itemFacade.GetAllMagicItemsAsync();
        
        FilterItems();
        IsBusy = false;
    }

    partial void OnSearchedItemChanged(string value)
    {
        FilterItems();
    }

    partial void OnItemCategoryChanged(ItemCategoriesEnum value)
    {
        FilterItems();
    }

    private void FilterItems()
    {
        var baseList = ItemCategory switch
        {
            ItemCategoriesEnum.Equipment => _allEquipmentApiList,
            ItemCategoriesEnum.MagicItem => _allMagicItemsApiList,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (!string.IsNullOrWhiteSpace(SearchedItem))
        {
            var filtered = baseList.Where(i => i.Name.Contains(SearchedItem, StringComparison.OrdinalIgnoreCase));
            EquipmentApiList = new ObservableCollection<ItemListApiModel>(filtered);
        }
        else
        {
            EquipmentApiList = new ObservableCollection<ItemListApiModel>(baseList);
        }
    }
    
    [RelayCommand]
    private async Task GetItemFromApiAsync()
    {
        if (SelectedItem == null) return;

        IsBusy = true;
        try
        {
            Item = ItemCategory switch
            {
                ItemCategoriesEnum.Equipment => await itemFacade.GetFromEquipmentApi(SelectedItem.Index),
                ItemCategoriesEnum.MagicItem => await itemFacade.GetFromMagicItemApi(SelectedItem.Index),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            Item.CharacterId = _characterId;
            Item.Category = ItemCategory.ToString();
            
            await Shell.Current.GoToAsync("..", new Dictionary<string, object>()
            {
                {"Item", Item}
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine(e);
            await Shell.Current.DisplayAlertAsync("API Error", e.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}