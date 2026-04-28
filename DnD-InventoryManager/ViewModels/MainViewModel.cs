using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;
using DnD_InventoryManager.Views;

namespace DnD_InventoryManager.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly DatabaseService _databaseService;
    private readonly NfcService _nfcService;
    public ObservableCollection<Character> Characters { get; } = new();
    [ObservableProperty] private bool _isWaitingForNfc;

    public MainViewModel(DatabaseService databaseService, NfcService nfcService)
    {
        _databaseService = databaseService;
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
        var list = await _databaseService.GetCharactersAsync();
        
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
    public void ListenForNfc()
    {
        IsWaitingForNfc = true;

        _nfcService.StartListening(
            onCharacterReceived: async (receivedCharacter) =>
            {
                _nfcService.StopListening();
                receivedCharacter.Id = 0;
                
                try
                {
                    await _databaseService.SaveCharacterAsync(receivedCharacter);
                
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        IsWaitingForNfc = false;
                        Characters.Add(receivedCharacter);
                        await Shell.Current.DisplayAlertAsync("Success", "Character saved", "OK");
                    });
                }
                catch (Exception ex)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        IsWaitingForNfc = false;
                        await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
                    });
                }
            },
            onError: (errorMsg) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    IsWaitingForNfc = false;
                    await Shell.Current.DisplayAlertAsync("Error", errorMsg, "OK");
                });
            });
    }

    [RelayCommand]
    private void CancelNfc()
    {
        IsWaitingForNfc = false;
        _nfcService.StopListening();
    }
    
    [RelayCommand]
    private async Task ShowDiceRollerAsync()
    {
        await Shell.Current.GoToAsync(nameof(DiceRollerPage));
    }

}