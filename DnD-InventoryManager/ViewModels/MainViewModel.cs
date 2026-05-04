using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Facades;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;
using DnD_InventoryManager.Views;

namespace DnD_InventoryManager.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly CharacterFacade _characterFacade;
    private readonly ItemFacade _itemFacade;
    private readonly NfcService _nfcService;
    
    public ObservableCollection<CharacterModel> Characters { get; } = [];
    
    [ObservableProperty]
    public partial bool IsWaitingForNfc { get; set; }

    public MainViewModel(CharacterFacade characterFacade, ItemFacade itemFacade , NfcService nfcService)
    {
        _characterFacade = characterFacade;
        _itemFacade =  itemFacade;
        _nfcService = nfcService;
        Title = "My Characters";
        
        LoadSamples();
    }

    private void LoadSamples()
    {
        Characters.Add(new CharacterModel() { Name = "Johb", Strength = 18, Size = CharacterSizeEnum.Medium});
    }

    [RelayCommand]
    private async Task GoToAddCharacterAsync()
    {
        await Shell.Current.GoToAsync(nameof(CharacterEditPage));
    }

    [RelayCommand]
    public async Task LoadCharactersAsync()
    {
        IsBusy = true;
        var list = await _characterFacade.GetAllAsync();
        
        Characters.Clear();
        foreach (var c in list)
        {
            Characters.Add(c);
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task GoToCharacterDetailAsync(CharacterModel selectedCharacter)
    {
        await Shell.Current.GoToAsync(nameof(CharacterDetailPage), new Dictionary<string, object>
        {
            { "Character", selectedCharacter }
        });
    }
    
    [RelayCommand]
    private async Task ListenForNfcAsync()
    {
        IsWaitingForNfc = true;

        _nfcService.StartListening(
            onItemModelReceived: (recievedItem) =>
            {
                _nfcService.StopListening();
                
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        IsWaitingForNfc = false;

                        if (Characters.Count == 0)
                        {
                            await Shell.Current.DisplayAlertAsync("Error", "No characters found", "OK");
                            return;
                        }

                        var characterNames = Characters.Select(c => c.Name).ToArray();
                        var selectedName = await Shell.Current.DisplayActionSheetAsync("Who gets this item?", "Cancel",
                            null, characterNames);

                        if (selectedName == "Cancel" || string.IsNullOrEmpty(selectedName))
                        {
                            return;
                        }

                        var selectedCharacter = Characters.FirstOrDefault(c => c.Name == selectedName);
                        recievedItem.CharacterId = selectedCharacter.Id;

                        await _itemFacade.SaveAsync(recievedItem);
                        await Shell.Current.DisplayAlertAsync("Success",
                            $"{recievedItem.Name} was added to {selectedCharacter.Name}'s inventory!", "OK");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"UI Error (Success): {ex.Message}");
                    }
                } );
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
        _nfcService.StopListening();
    }
    
    private static async Task ShowDiceRollerAsync()
    {
        await Shell.Current.GoToAsync(nameof(DiceRollerPage));
    }
}