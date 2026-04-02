using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;

namespace DnD_InventoryManager.ViewModels;

public partial class AddCharacterViewModel : ViewModelBase
{
    [ObservableProperty] private string name = string.Empty;

    [ObservableProperty] private int strength = 10;

    [ObservableProperty] private CharacterSizeEnum selectedSize = CharacterSizeEnum.Medium;

    [ObservableProperty] private string selectedImagePath = "dotnet_bot.png";

    private readonly DatabaseService _databaseService;

    public List<CharacterSizeEnum> AllSizes =>
        Enum.GetValues(typeof(CharacterSizeEnum)).Cast<CharacterSizeEnum>().ToList();

    public AddCharacterViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "New Character";
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

        var newCharacter = new Character
        {
            Name = Name,
            Strength = Strength,
            Size = SelectedSize,
            ImagePath = selectedImagePath
        };

        await _databaseService.SaveCharacterAsync(newCharacter);
        await Shell.Current.GoToAsync("..");
    }
}