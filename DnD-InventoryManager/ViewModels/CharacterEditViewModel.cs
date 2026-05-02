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

    [ObservableProperty]
    public partial CharacterModel? CharacterToEdit { get; set; }
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int Strength { get; set; } = 10;

    [ObservableProperty]
    public partial CharacterSizeEnum SelectedSize { get; set; } = CharacterSizeEnum.Medium;

    [ObservableProperty]
    public partial string SelectedImagePath { get; set; } = "dotnet_bot.png";

    public static List<CharacterSizeEnum> AllSizes =>
        Enum.GetValues<CharacterSizeEnum>().Cast<CharacterSizeEnum>().ToList();

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
            var mediaOptions = new MediaPickerOptions
            {
                SelectionLimit = 1
            };
            
            var photos = await MediaPicker.Default.PickPhotosAsync(mediaOptions);
            
            var photo = photos.FirstOrDefault();
            
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
        Strength = value switch
        {
            < 1 => 1,
            > 30 => 30,
            _ => Strength
        };
    }
}