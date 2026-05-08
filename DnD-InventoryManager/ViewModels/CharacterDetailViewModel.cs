using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Facades;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;
using DnD_InventoryManager.Views;

namespace DnD_InventoryManager.ViewModels;

[QueryProperty(nameof(Character), "Character")]
public partial class CharacterDetailViewModel(
    CharacterFacade characterFacade,
    ItemFacade itemFacade,
    NfcService nfcService
) : ViewModelBase
{
    private const string NoFilter = "No Filter";
    
    private ICollection<ItemModel> _allItems = [];

    [ObservableProperty]
    public partial CharacterModel? Character { get; set; }
    [ObservableProperty]
    public partial bool IsWaitingForNfc { get; set; }
    [ObservableProperty]
    public partial ObservableCollection<ItemModel> Items { get; set; } = [];
    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;
    [ObservableProperty] 
    public partial IList<string> AllCategories { get; set; } = [];
    [ObservableProperty] 
    public partial string CategoryFilter { get; set; } = NoFilter;
    [ObservableProperty]
    public partial double CurrentLoad { get; set; }
    [ObservableProperty]
    public partial double CurrentLoadPercentage { get; set; }
    
    public async Task RefreshCharacterAsync()
    {
        if (Character == null || Character.Id == 0)
        {
            return;
        }

        Items.Clear();
        var updatedCharacter = await characterFacade.GetByIdAsync(Character.Id);

        if (updatedCharacter != null)
        {
            Character = updatedCharacter;
            Title = Character.Name;
            
            _allItems = await itemFacade.GetAllByCharacterIdAsync(updatedCharacter.Id);
            AllCategories = [NoFilter, .. _allItems.Select(i => i.Category).Distinct().ToList()];
            

            CurrentLoad = _allItems.Sum(i => i.Weight * i.Quantity);
            CurrentLoadPercentage = Character.CarryingCapacity > 0 ? CurrentLoad / Character.CarryingCapacity : 0;
            
            FilterItems();
        }
    }

    partial void OnCharacterChanged(CharacterModel? value)
    {
        if (value != null)
        {
            Title = value.Name;
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        FilterItems();
    }

    partial void OnCategoryFilterChanged(string value)
    {
        FilterItems();
    }
    
    private void FilterItems()
    {
        IEnumerable<ItemModel> filtered;

        if (string.IsNullOrWhiteSpace(CategoryFilter) || CategoryFilter.Equals(NoFilter))
        {
            filtered = _allItems;
        }
        else
        {
            filtered = _allItems.Where(i => i.Category.Equals(CategoryFilter, StringComparison.CurrentCultureIgnoreCase));
        }

        if (!string.IsNullOrEmpty(SearchQuery))
        {
            filtered = filtered.Where(i => i.Name.Contains(SearchQuery, StringComparison.CurrentCultureIgnoreCase));
        }

        Items = new ObservableCollection<ItemModel>(filtered);
    }

    [RelayCommand]
    private async Task GoToEditCharacterAsync()
    {
        if (Character == null)
        {
            return;
        }
        
        await Shell.Current.GoToAsync(nameof(CharacterEditPage), new Dictionary<string, object>
        {
            { "Character", Character }
        });
    }


    [RelayCommand]
    private async Task DeleteCharacterAsync()
    {
        if (Character == null || Character.Id == 0)
        {
            return;
        }
        
        var answer = await Shell.Current.DisplayAlertAsync("Delete Character", "Are you sure you want to delete this character?", "Yes", "No");
        
        if (answer)
        {
            await itemFacade.DeleteAllByCharacterId(Character.Id);
            await characterFacade.DeleteAsync(Character.Id);
            await Shell.Current.GoToAsync("..");
        }
    }
    
    [RelayCommand]
    private void ListenForNfc()
    {
        if (Character == null) return;

        IsWaitingForNfc = true;
        
        nfcService.StartListening(
            onItemModelReceived: (recievedItem) =>
            {
                nfcService.StopListening();
                
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        IsWaitingForNfc = false;

                        var allCategories = await itemFacade.GetAllCategories();
                        var selectedCategory = await Shell.Current.DisplayActionSheetAsync(
                            $"Choose category for the new item {recievedItem.Name}?", 
                            "Cancel", 
                            null, 
                            allCategories.ToArray()
                        );
                        if (string.IsNullOrEmpty(selectedCategory) || selectedCategory.Equals("Cancel"))
                        {
                            selectedCategory = nameof(ItemCategoriesEnum.Equipment);
                        }
                        recievedItem.Category = selectedCategory;
                        recievedItem.CharacterId = Character.Id;

                        await itemFacade.SaveAsync(recievedItem);
                        
                        _allItems.Add(recievedItem);
                        FilterItems();
                        
                        CurrentLoad += (recievedItem.Weight * recievedItem.Quantity);
                        CurrentLoadPercentage = Character.CarryingCapacity > 0 ? CurrentLoad / Character.CarryingCapacity : 0;
                        
                        await Shell.Current.DisplayAlertAsync("Loot acquired!",
                            $"{recievedItem.Name} added to inventory.", "OK");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"UI Error (Success): {ex.Message}");
                    }
                });
            },
            onError: (errorMsg) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    IsWaitingForNfc = false;
                    await Shell.Current.DisplayAlertAsync("Error", errorMsg, "OK");
                });
            }
            );
    }

    [RelayCommand]
    private void CancelNfc()
    {
        IsWaitingForNfc = false;
        nfcService.StopListening();
    }

    [RelayCommand]
    private async Task GoToAddItemAsync()
    {
        if (Character == null)
        {
            return;
        }
        await Shell.Current.GoToAsync(nameof(ItemEditPage), new Dictionary<string, object>()
        {
            { "Item", new ItemModel{CharacterId = Character.Id} }
        });
    }

    [RelayCommand]
    private async Task GoToItemDetailAsync(ItemModel item)
    {
        if (Character == null)
        {
            return;
        }

        await Shell.Current.GoToAsync(nameof(ItemDetailPage), new Dictionary<string, object>()
        {
            { "Item", item }
        });
    }
    
    [RelayCommand]
    private async Task ScanItemQrAsync()
    {
        if (Character == null) return;

        await Shell.Current.GoToAsync(nameof(QrScanPage), new Dictionary<string, object>
        {
            { "CharacterId", Character.Id }
        });
    }

    
    
}