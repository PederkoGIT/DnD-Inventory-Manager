using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;
using DnD_InventoryManager.Views;

namespace DnD_InventoryManager.ViewModels;

[QueryProperty(nameof(Character), "Character")]
public partial class CharacterDetailViewModel : ViewModelBase
{
    [ObservableProperty] private Character? character;

    private readonly DatabaseService _databaseService;
    private readonly NfcService _nfcService;

    public CharacterDetailViewModel(DatabaseService databaseService, NfcService nfcService)
    {
        _databaseService = databaseService;
        _nfcService = nfcService;
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