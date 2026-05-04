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
    [ObservableProperty]
    public partial CharacterModel? Character { get; set; }

    [ObservableProperty]
    public partial bool IsWaitingForNfc { get; set; }

    public ObservableCollection<ItemModel> Items { get; } = [];
    
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
            var items = await itemFacade.GetAllByCharacterIdAsync(updatedCharacter.Id);
            foreach (var item in items)
            {
                Items.Add(item);
            }
            Title = Character.Name;
        }
    }

    partial void OnCharacterChanged(CharacterModel? value)
    {
        if (value != null)
        {
            Title = value.Name;
        }
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
            await characterFacade.DeleteAsync(Character.Id);
            
            await Shell.Current.GoToAsync("..");
        }
    }
    
    [RelayCommand]
    private void WriteToNfc()
    {
        if (Character == null) return;

        IsWaitingForNfc = true;
        
        nfcService.StartWriting(Character,
            onSuccess: () =>
            {
                MainThread.BeginInvokeOnMainThread(async void () =>
                {
                    try
                    {
                        IsWaitingForNfc = false;
                        await Shell.Current.DisplayAlertAsync("Success", "Character written to NFC tag", "OK");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Success popup fail: {ex.Message}");
                    }
                });
            },
            onError: (errorMsg) =>
            {
                MainThread.BeginInvokeOnMainThread(async void () => 
                {
                    try
                    {
                        IsWaitingForNfc = false;
                        await Shell.Current.DisplayAlertAsync("Error", errorMsg, "OK");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error popup fail: {ex.Message}");
                    }
                });
            });
    }

    [RelayCommand]
    private void CancelNfc()
    {
        IsWaitingForNfc = false;
        nfcService.StopWriting();
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
    private async Task ShowQrCodeAsync()
    {
        if (Character == null) return;

        await Shell.Current.GoToAsync(nameof(QrCodeDisplayPage), new Dictionary<string, object>
        {
            { "Character", Character }
        });
    }

    
    
}