using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Facades;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;
using DnD_InventoryManager.Views;

namespace DnD_InventoryManager.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly CharacterFacade _characterFacade;
    private readonly NfcService _nfcService;
    public ObservableCollection<Character> Characters { get; } = new();

    public MainViewModel(CharacterFacade characterFacade, NfcService nfcService)
    {
        _characterFacade = characterFacade;
        _nfcService = nfcService;
        Title = "My Characters";
        
        LoadSamples();
    }

    private void LoadSamples()
    {
        Characters.Add(new Character { Name = "Johb", Strength = 18, Size = CharacterSizeEnum.Medium});
    }

    [RelayCommand]
    private async Task GoToAddCharacterAsync()
    {
        await Shell.Current.GoToAsync(nameof(EditCharacterPage));
    }

    [RelayCommand]
    public async Task LoadCharactersAsync()
    {
        IsBusy = true;
        var list = await _characterFacade.GetAllAsync();
        
        Characters.Clear();
        foreach (var c in list)
            Characters.Add(c);

        IsBusy = false;
    }

    [RelayCommand]
    public async Task GoToCharacterDetailAsync(Character selectedCharacter)
    {
        if (selectedCharacter == null)
        {
            return;
        }

        await Shell.Current.GoToAsync(nameof(CharacterDetailPage), new Dictionary<string, object>
        {
            { "Character", selectedCharacter }
        });
    }
    
    [RelayCommand]
    public async Task ListenForNfcAsync()
    {
        _nfcService.StartListening(
            onCharacterReceived: async (receivedCharacter) =>
            {
                _nfcService.StopListening();
                receivedCharacter.Id = 0;
                
                try
                {
                    await _characterFacade.SaveAsync(receivedCharacter);
                
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        Characters.Add(receivedCharacter);
                        await Shell.Current.DisplayAlertAsync("Success", "Character saved", "OK");
                    });
                }
                catch (Exception ex)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
                    });
                }
            },
            onError: (errorMsg) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Shell.Current.DisplayAlertAsync("Error", errorMsg, "OK");
                });
            });

        await Shell.Current.DisplayAlertAsync("Listen for NFC", "Attach your phone to NFC tag", "OK");
    }
    
    [RelayCommand]
    private async Task ShowDiceRollerAsync()
    {
        await Shell.Current.GoToAsync(nameof(DiceRollerPage));
    }

}