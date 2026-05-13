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
    private readonly Dictionary<string, string> _pendingCategoryRenames = [];
    
    
    [ObservableProperty] public partial ItemModel ItemModel { get; set; } = new() ;
    [ObservableProperty] public partial double NewWeight { get; set; }
    [ObservableProperty] public partial int NewQuantity { get; set; } = 1;
    [ObservableProperty] public partial string? PickerCategory { get; set; } = string.Empty;
    [ObservableProperty] public partial List<string> AllCategories { get; set; } = [];
    [ObservableProperty] public partial string SelectedImagePath { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsCategoryMenuVisible { get; set; } = false;

    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(CanDeleteCategory))]
    public partial string NewCategory { get; set; } = string.Empty;
    
    
    [ObservableProperty] public partial bool IsPromptVisible { get; set; }
    [ObservableProperty] public partial string PromptTitle { get; set; } = string.Empty;
    [ObservableProperty] public partial string PromptMessage { get; set; } = string.Empty;
    [ObservableProperty] public partial string PromptInputText { get; set; } = string.Empty;
    [ObservableProperty] public partial string PromptConfirmText { get; set; } = "Save";
    private TaskCompletionSource<string?>? _promptTcs;

    [ObservableProperty] public partial bool IsAlertVisible { get; set; }
    [ObservableProperty] public partial string AlertTitle { get; set; } = string.Empty;
    [ObservableProperty] public partial string AlertMessage { get; set; } = string.Empty;
    [ObservableProperty] public partial string AlertConfirmText { get; set; } = "OK";
    private TaskCompletionSource<bool>? _alertTcs;

    [ObservableProperty] public partial bool IsSelectCategoryVisible { get; set; }
    [ObservableProperty] public partial List<string> DisplayCategories { get; set; } = [];
    private TaskCompletionSource<string?>? _actionSheetTcs;
    
    public async Task LoadDataAsync()
    {
        var defaultCategories = Enum.GetValues<ItemCategoriesEnum>().Select(e => e.ToString());
        var dbCategories = await itemFacade.GetAllCategories();
    
        AllCategories = defaultCategories.Union(dbCategories)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToList();

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

        foreach (var rename in _pendingCategoryRenames)
        {
            await itemFacade.RenameCategoryAsync(rename.Key, rename.Value);
        }
        
        foreach (var categoryToDelete in _pendingCategoryDeletions)
        {
            await itemFacade.DeleteCategoryAndReassignAsync(categoryToDelete, "Uncategorized");
        }
        
        _pendingCategoryDeletions.Clear();
        _pendingCategoryRenames.Clear();
        
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
        var categoriesToDisplay = AllCategories.ToList();
        if (!categoriesToDisplay.Contains("Uncategorized"))
            categoriesToDisplay.Add("Uncategorized");
            
        DisplayCategories = categoriesToDisplay;
        
        IsSelectCategoryVisible = true;
        _actionSheetTcs = new TaskCompletionSource<string?>();
        var action = await _actionSheetTcs.Task;

        if (!string.IsNullOrEmpty(action))
        {
            NewCategory = action;
        }
    }

    [RelayCommand]
    private async Task AddCategoryAsync()
    {
        IsCategoryMenuVisible = false;
        
        var newCategoryName = await ShowCustomPromptAsync("New Category", "Enter the name of the new category:", "Save");

        if (!string.IsNullOrWhiteSpace(newCategoryName) && !AllCategories.Contains(newCategoryName))
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

        IsCategoryMenuVisible = false;
        var categoryToDelete = NewCategory;
    
        var confirm = await ShowCustomAlertAsync("Delete Category", 
            $"Are you sure you want to mark '{categoryToDelete}' for deletion? All items in this category will be moved to 'Uncategorized' when you save.", 
            "Mark for Deletion");

        if (confirm)
        {
            if (!_pendingCategoryDeletions.Contains(categoryToDelete))
                _pendingCategoryDeletions.Add(categoryToDelete);

            AllCategories.Remove(categoryToDelete);
            OnPropertyChanged(nameof(AllCategories));
            NewCategory = "Uncategorized";
        }
    }

    [RelayCommand]
    private void OpenCategoryMenu() => IsCategoryMenuVisible = true;
    
    [RelayCommand]
    private void CloseCategoryMenu() => IsCategoryMenuVisible = false;

    [RelayCommand]
    private async Task RenameCategoryAsync()
    {
        IsCategoryMenuVisible = false;
        
        var newName = await ShowCustomPromptAsync("Rename Category", $"Rename '{NewCategory}' to:", "Rename", NewCategory);

        if (!string.IsNullOrWhiteSpace(newName) && newName.Trim() != NewCategory)
        {
            var cleanName = newName.Trim();
            var oldName = NewCategory;

            _pendingCategoryRenames[oldName] = cleanName;

            AllCategories.Remove(oldName);
            AllCategories.Add(cleanName);
            OnPropertyChanged(nameof(AllCategories));

            NewCategory = cleanName;
        }
    }
    
    private async Task<string?> ShowCustomPromptAsync(string title, string message, string confirmText, string initialValue = "")
    {
        PromptTitle = title;
        PromptMessage = message;
        PromptConfirmText = confirmText;
        PromptInputText = initialValue;
        IsPromptVisible = true;
        
        _promptTcs = new TaskCompletionSource<string?>();
        return await _promptTcs.Task;
    }

    [RelayCommand] private void ConfirmPrompt() { IsPromptVisible = false; _promptTcs?.TrySetResult(PromptInputText); }
    [RelayCommand] private void CancelPrompt() { IsPromptVisible = false; _promptTcs?.TrySetResult(null); }

    private async Task<bool> ShowCustomAlertAsync(string title, string message, string confirmText)
    {
        AlertTitle = title;
        AlertMessage = message;
        AlertConfirmText = confirmText;
        IsAlertVisible = true;
        
        _alertTcs = new TaskCompletionSource<bool>();
        return await _alertTcs.Task;
    }

    [RelayCommand] private void ConfirmAlert() { IsAlertVisible = false; _alertTcs?.TrySetResult(true); }
    [RelayCommand] private void CancelAlert() { IsAlertVisible = false; _alertTcs?.TrySetResult(false); }

    [RelayCommand] private void PickCategoryOption(string option) { IsSelectCategoryVisible = false; _actionSheetTcs?.TrySetResult(option); }
    [RelayCommand] private void CancelCategorySheet() { IsSelectCategoryVisible = false; _actionSheetTcs?.TrySetResult(null); }
}