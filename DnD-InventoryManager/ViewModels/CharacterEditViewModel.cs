using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Facades;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;

namespace DnD_InventoryManager.ViewModels;

[QueryProperty(nameof(CharacterToEdit), "Character")]
public partial class CharacterEditViewModel : ViewModelBase
{
    private readonly CharacterFacade _characterFacade;

    [ObservableProperty] private CharacterModel? characterToEdit;
    
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private int strength = 10;
    [ObservableProperty] private CharacterSizeEnum selectedSize = CharacterSizeEnum.Medium;
    [ObservableProperty] private string selectedImagePath = "dotnet_bot.png";


    public List<CharacterSizeEnum> AllSizes =>
        Enum.GetValues(typeof(CharacterSizeEnum)).Cast<CharacterSizeEnum>().ToList();

    public CharacterEditViewModel(CharacterFacade characterFacade)
    {
        _characterFacade = characterFacade;
        Title = "New Character";
    }

    partial void OnCharacterToEditChanged(CharacterModel? value)
    {
        if (value == null)
        {
            return;
        }
        
        Name = value.Name;
        Strength =  value.Strength;
        SelectedSize = value.Size;
        SelectedImagePath =  value.ImagePath;
        Title = "Edit Character";
    }

    [RelayCommand]
    private async Task PickImageAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo != null)
            {
                SelectedImagePath = photo.FullPath;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error from image picker: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;

        var characterToSave = CharacterToEdit ?? new CharacterModel();

        characterToSave.Name = Name;
        characterToSave.Strength = Strength;
        characterToSave.ImagePath = SelectedImagePath;
        characterToSave.Size = SelectedSize;

        await _characterFacade.SaveAsync(characterToSave);
        await Shell.Current.GoToAsync("..");
    }

    partial void OnStrengthChanged(int value)
    {
        if (value < 1)
        {
            Strength = 1;
        }

        if (value > 30)
        {
            Strength = 30;
        }
    }
}