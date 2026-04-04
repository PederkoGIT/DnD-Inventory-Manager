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

    public CharacterDetailViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
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
    
    
}