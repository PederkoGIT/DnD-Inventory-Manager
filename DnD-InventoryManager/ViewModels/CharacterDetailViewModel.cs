using CommunityToolkit.Mvvm.ComponentModel;
using DnD_InventoryManager.Models;

namespace DnD_InventoryManager.ViewModels;

[QueryProperty(nameof(Character), "Character")]
public partial class CharacterDetailViewModel : ViewModelBase
{
    [ObservableProperty] private Character? character;

    public CharacterDetailViewModel()
    {
        Title = "Detail";
    }

    partial void OnCharacterChanged(Character? value)
    {
        if (value != null)
        {
            Title = value.Name;
        }
    }
    
    
}