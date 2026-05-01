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
    
    public ObservableCollection<Character> Characters { get; } = [];
    
    [ObservableProperty]
    public partial bool IsWaitingForNfc { get; set; }

    public MainViewModel(DatabaseService databaseService, NfcService nfcService)
    {
        _databaseService = databaseService;
        _nfcService = nfcService;
        Title = "My Characters";
        
        LoadSamples();
    }

    private void LoadSamples()
    {
        Characters.Add(new() { Name = "Johb", Strength = 18, Size = CharacterSizeEnum.Medium});
    }

    [RelayCommand]
    private static async Task GoToAddCharacterAsync()
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
        {
            Characters.Add(c);
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task GoToCharacterDetailAsync(Character selectedCharacter)
    {
        await Shell.Current.GoToAsync(nameof(CharacterDetailPage), new Dictionary<string, object>
        {
            { "Character", selectedCharacter }
        });
    }
    
    [RelayCommand]
    private void ListenForNfc()
    {
        IsWaitingForNfc = true;

        _nfcService.StartListening(
            onCharacterReceived: async void (receivedCharacter) =>
            {
                try
                {
                    _nfcService.StopListening();
                    receivedCharacter.Id = 0;
                    
                    await _databaseService.SaveCharacterAsync(receivedCharacter);
            
                    MainThread.BeginInvokeOnMainThread(async void () =>
                    {
                        try
                        {
                            IsWaitingForNfc = false;
                            Characters.Add(receivedCharacter);
                            await Shell.Current.DisplayAlertAsync("Success", "Character saved", "OK");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"UI Error (Success): {ex.Message}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    MainThread.BeginInvokeOnMainThread(async void () =>
                    {
                        try 
                        {
                            IsWaitingForNfc = false;
                            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
                        }
                        catch (Exception innerEx)
                        {
                            Console.WriteLine($"UI Error (DB Fail): {innerEx.Message}");
                        }
                    });
                }
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
                        Console.WriteLine($"UI Error (NFC Fail): {ex.Message}");
                    }
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
    private static async Task ShowDiceRollerAsync()
    {
        await Shell.Current.GoToAsync(nameof(DiceRollerPage));
    }
}