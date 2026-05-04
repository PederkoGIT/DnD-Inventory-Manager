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
    [ObservableProperty] public partial ItemModel ItemModel { get; set; } = new() ;
    [ObservableProperty] public partial double NewWeight { get; set; }
    [ObservableProperty] public partial int NewQuantity { get; set; } = 1;

    public async Task LoadDataAsync()
    {
        Title = "Add Item";
        var itemFromDb = await itemFacade.GetByIdAsync(ItemModel.Id);
        if (itemFromDb is not null)
        {
            Title = "Edit Item";
            ItemModel = itemFromDb;
            NewWeight = itemFromDb.Weight;
            NewQuantity = itemFromDb.Quantity;
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
            < 0 => 0,
            > 999 => 999,
            _ => NewQuantity
        };
    }

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
                ItemModel.ImagePath = photo.FullPath;
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
        ItemModel.Weight = NewWeight;
        ItemModel.Quantity = NewQuantity;
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
}