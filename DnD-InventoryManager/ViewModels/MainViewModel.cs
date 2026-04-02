using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Views;

namespace DnD_InventoryManager.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ObservableCollection<Character> Characters { get; } = new();

    public MainViewModel()
    {
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
        await Shell.Current.GoToAsync(nameof(AddCharacterPage));
    }

}