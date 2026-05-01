using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Api;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;
using DnD_InventoryManager.Views;

namespace DnD_InventoryManager.ViewModels;

[QueryProperty(nameof(Character), "Character")]
public partial class CharacterDetailViewModel : ViewModelBase
{
    [ObservableProperty] private Character? character;

    public ObservableCollection<Item> Items { get; } = new();
    
    private readonly DatabaseService _databaseService;
    private readonly NfcService _nfcService;
    private readonly ItemService _itemService;
    
    public CharacterDetailViewModel(DatabaseService databaseService, NfcService nfcService,  ItemService itemService)
    {
        _databaseService = databaseService;
        _nfcService = nfcService;
        _itemService = itemService;
        Title = "Detail";
    }

    public async Task RefreshCharacterAsync()
    {
        if (Character == null || Character.Id == 0)
        {
            return;
        }

        var updatedCharacter = await _databaseService.GetCharacterById(Character.Id);

        if (updatedCharacter != null)
        {
            Character = updatedCharacter;
            Title = Character.Name;
        }

        Items.Add(await _itemService.GetEquipmentFromApiAsync("chariot"));
        Items.Add(await _itemService.GetMagicItemFromApiAsync("adamantine-armor"));
    }

    partial void OnCharacterChanged(Character? value)
    {
        if (value != null)
        {
            Title = value.Name;
        }
    }

    [RelayCommand]
    private async Task GoToEditCharacterAsync()
    {
        if (character == null)
        {
            return;
        }
        
        await Shell.Current.GoToAsync(nameof(EditCharacterPage), new Dictionary<string, object>
        {
            { "Character", character }
        });
    }


    [RelayCommand]
    private async Task DeleteCharacterAsync()
    {
        if (Character == null || Character.Id == 0)
        {
            return;
        }
        
        bool answer = await Shell.Current.DisplayAlertAsync("Delete Character", "Are you sure you want to delete this character?", "Yes", "No");
        
        if (answer)
        {
            await _databaseService.DeleteCharacterAsync(Character.Id);
            
            await Shell.Current.GoToAsync("..");
        }
    }
    
    [RelayCommand]
    private async Task WriteToNfcAsync()
    {
        if (Character == null) return;

        _nfcService.StartWriting(Character,
            onSuccess: () =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Shell.Current.DisplayAlertAsync("Success", "Character written to NFC tag", "OK");
                });
            },
            onError: (errorMsg) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Shell.Current.DisplayAlertAsync("Error", errorMsg, "OK");
                });
            });

        await Shell.Current.DisplayAlertAsync("Write to NFC", "Attach your phone to NFC tag", "OK");
    }
    
    
}