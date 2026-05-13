using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Facades;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Views;

namespace DnD_InventoryManager.ViewModels;

[QueryProperty(nameof(ItemModel), "Item")]
public partial class ItemEditViewModel(
    ItemFacade itemFacade
    ) : ViewModelBase
{
    
    private static readonly string[] DefaultItemImages = ["armor.png", "sword.png", "potion.png"];
    private readonly List<string> _pendingCategoryDeletions = [];
    
    [ObservableProperty] public partial ItemModel ItemModel { get; set; } = new() ;
    [ObservableProperty] public partial double NewWeight { get; set; }
    [ObservableProperty] public partial int NewQuantity { get; set; } = 1;
    [ObservableProperty] public partial string? PickerCategory { get; set; } = string.Empty;
    [ObservableProperty] public partial List<string> AllCategories { get; set; } = [];
    [ObservableProperty] public partial string SelectedImagePath { get; set; } = string.Empty;

    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(CanDeleteCategory))]
    public partial string NewCategory { get; set; } = string.Empty;
    
    public async Task LoadDataAsync()
    {
        var defaultCategories = Enum.GetValues<ItemCategoriesEnum>().Select(e => e.ToString());
        var dbCategories = await itemFacade.GetAllCategories();
    
        AllCategories = defaultCategories.Union(dbCategories).ToList();

        var itemFromDb = await itemFacade.GetByIdAsync(ItemModel.Id);
    
        if (itemFromDb is not null)
        {
            Title = "Edit Item";
            ItemModel = itemFromDb;
            NewWeight = itemFromDb.Weight;
            NewQuantity = itemFromDb.Quantity < 1 ? 1 : itemFromDb.Quantity;
            NewCategory = itemFromDb.Category;
            SelectedImagePath = itemFromDb.ImagePath;
        }
        else
        {
            Title = "Add Item";
            NewWeight = ItemModel.Weight;
            NewQuantity = 1;
            NewCategory = string.IsNullOrEmpty(ItemModel.Category) ? nameof(ItemCategoriesEnum.Equipment) : ItemModel.Category;
            
            if (!string.IsNullOrEmpty(ItemModel.ImagePath) && ItemModel.ImagePath.StartsWith("http"))
            {
                SelectedImagePath = ItemModel.ImagePath;
            }
            else
            {
                SelectedImagePath = DefaultItemImages[Random.Shared.Next(DefaultItemImages.Length)];
            }
        }
    }
    
    partial void OnNewWeightChanged(double value)
    {
        NewWeight = value switch
        {
            < 0 => 0,
            > 999 => 999,
            _ => NewWeight
        };
    }

    partial void OnNewQuantityChanged(int value)
    {
        NewQuantity = value switch
        {
            < 1 => 1,
            > 999 => 999,
            _ => NewQuantity
        };
    }

    partial void OnPickerCategoryChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            NewCategory = value;
            
            MainThread.BeginInvokeOnMainThread(() => PickerCategory = null);
        }
    }

    public bool CanDeleteCategory =>
        !string.IsNullOrEmpty(NewCategory) &&
        NewCategory != nameof(ItemCategoriesEnum.Equipment) &&
        NewCategory != "MagicItem" &&
        NewCategory != "Uncategorized";
    
    [RelayCommand]
    private async Task PickImageAsync()
    {
        try
        {
            var mediaOptions = new MediaPickerOptions
            {
                SelectionLimit = 1
            };
            
            var photos = await MediaPicker.Default.PickPhotosAsync(mediaOptions);
            
            var photo = photos.FirstOrDefault();
            
            if (photo != null)
            {
                SelectedImagePath = photo.FullPath;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error from image picker: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private async Task SaveAsync()
    {
        foreach (var categoryToDelete in _pendingCategoryDeletions)
        {
            await itemFacade.DeleteCategoryAndReassignAsync(categoryToDelete, "Uncategorized");
        }
        _pendingCategoryDeletions.Clear();
        
        ItemModel.Weight = NewWeight;
        ItemModel.Quantity = NewQuantity;
        ItemModel.Category = NewCategory;
        ItemModel.ImagePath = SelectedImagePath;
        
        await itemFacade.SaveAsync(ItemModel);
        await Shell.Current.GoToAsync("..");
    }
    
    [RelayCommand]
    private async Task GetItemFromApi()
    {
        await Shell.Current.GoToAsync(nameof(ItemFromApiPage), new Dictionary<string, object>()
        {
            {"Item", ItemModel}
        });
    }

    [RelayCommand]
    private async Task SelectCategoryAsync()
    {
        var displayCategories = AllCategories.ToList();
        if (!displayCategories.Contains("Uncategorized"))
        {
            displayCategories.Add("Uncategorized");
        }
        
        var action = await Shell.Current.DisplayActionSheetAsync("Select Category", "Cancel", null, displayCategories.ToArray());

        if (!string.IsNullOrEmpty(action) && action != "Cancel")
        {
            NewCategory = action;
        }
    }

    [RelayCommand]
    private async Task AddCategoryAsync()
    {
        var newCategoryName = await Shell.Current.DisplayPromptAsync("New Category", "Enter the name of the new category:", "Save", "Cancel");

        if (!string.IsNullOrEmpty(newCategoryName) && !AllCategories.Contains(newCategoryName))
        {
            AllCategories.Add(newCategoryName.Trim());
            
            OnPropertyChanged(nameof(AllCategories));
            
            NewCategory = newCategoryName.Trim();
        }
    }
    
    [RelayCommand]
    private async Task DeleteCategoryAsync()
    {
        if (!CanDeleteCategory) return;

        var categoryToDelete = NewCategory;
    
        var confirm = await Shell.Current.DisplayAlertAsync(
            "Delete Category", 
            $"Are you sure you want to mark '{categoryToDelete}' for deletion? All items in this category will be moved to 'Uncategorized' when you save.", 
            "Mark for Deletion", "Cancel");

        if (confirm)
        {
            if (!_pendingCategoryDeletions.Contains(categoryToDelete))
            {
                _pendingCategoryDeletions.Add(categoryToDelete);
            }

            AllCategories.Remove(categoryToDelete);
            OnPropertyChanged(nameof(AllCategories));

            NewCategory = "Uncategorized";
        }
    }
}