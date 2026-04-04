using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;
using DnD_InventoryManager.Views;

namespace DnD_InventoryManager.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly DatabaseService _databaseService;
    public ObservableCollection<Character> Characters { get; } = new();

    public MainViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
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

}