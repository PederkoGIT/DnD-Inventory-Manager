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
    public partial ICollection<ItemListApiModel> EquipmentApiList { get; set; } = [];
    
    [ObservableProperty] 
    public partial string SearchedItem { get; set; } = string.Empty;
    
    [ObservableProperty] 
    public partial ItemModel Item { get; set; } = new();

    [ObservableProperty] 
    public partial ItemListApiModel SelectedItem { get; set; } = new();
    
    [ObservableProperty]
    public partial ItemCategoriesEnum ItemCategory { get; set; }

    public async Task LoadPage()
    {
        _characterId = Item.CharacterId;
        _allEquipmentApiList = await itemFacade.GetAllEquipmentApiAsync();
        _allMagicItemsApiList = await itemFacade.GetAllMagicItemsAsync();
        EquipmentApiList = _allEquipmentApiList.ToList();
    }
    
    public void OnSearchedItemChanged()
    {

        EquipmentApiList = ItemCategory switch
        {
            ItemCategoriesEnum.Equipment => _allEquipmentApiList.ToList(),
            ItemCategoriesEnum.MagicItem => _allMagicItemsApiList.ToList(),
            _ => throw new ArgumentOutOfRangeException()
        };

        if (!SearchedItem.Equals(string.Empty))
        {
            EquipmentApiList = EquipmentApiList.Where(i => i.Name.Contains(SearchedItem, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }
    
    }
    
    [RelayCommand]
    private async Task GetItemFromApiAsync()
    {
        try
        {
            Item = ItemCategory switch
            {
                ItemCategoriesEnum.Equipment => await itemFacade.GetFromEquipmentApi(SelectedItem.Index),
                ItemCategoriesEnum.MagicItem => await itemFacade.GetFromMagicItemApi(SelectedItem.Index),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (ApiException e)
        {
            Console.WriteLine(e);
            await Shell.Current.DisplayAlertAsync("Error", e.Message, "OK");
        }
        
        Item.Category = ItemCategory.ToString();
        Item.CharacterId = _characterId;
        
        await Shell.Current.GoToAsync("..", new Dictionary<string, object>()
        {
            {"Item", Item}
        });
    }
}