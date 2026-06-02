using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Facades;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;
using DnD_InventoryManager.Views;

namespace DnD_InventoryManager.ViewModels;

public partial class TemplateImageItem : ObservableObject
{
    [ObservableProperty] public partial string ImagePath { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsSelected { get; set; }
}

public partial class MainViewModel : ViewModelBase
{
    private readonly CharacterFacade _characterFacade;
    private readonly ItemFacade _itemFacade;
    private readonly NfcService _nfcService;
    
    public ObservableCollection<CharacterModel> Characters { get; } = [];
    
    [ObservableProperty]
    public partial bool IsWaitingForNfc { get; set; }
    
    [ObservableProperty] public partial bool IsItemPreviewVisible { get; set; }
    [ObservableProperty] public partial ItemModel? PreviewItem { get; set; }
    [ObservableProperty] public partial CharacterModel? PreviewSelectedCharacter { get; set; }
    [ObservableProperty] public partial string PreviewSelectedImage { get; set; } = string.Empty;
    [ObservableProperty] public partial ObservableCollection<TemplateImageItem> PreviewTemplateImages { get; set; } = [];
    private TaskCompletionSource<bool>? _previewTcs;

    [ObservableProperty] public partial bool IsCharacterSelectVisible { get; set; }
    [ObservableProperty] public partial bool IsPreviewCategorySelectVisible { get; set; }
    [ObservableProperty] public partial string PreviewSelectedCategory { get; set; } = string.Empty;
    [ObservableProperty] public partial List<string> PreviewAvailableCategories { get; set; } = [];

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
                            await Shell.Current.DisplayAlertAsync("Error", "No characters found. Create one first.", "OK");
                            return;
                        }

                        var confirm = await ShowItemPreviewAsync(recievedItem);

                        if (confirm && PreviewSelectedCharacter != null)
                        {
                            recievedItem.CharacterId = PreviewSelectedCharacter.Id;
                            recievedItem.ImagePath = PreviewSelectedImage;
                            recievedItem.Category = PreviewSelectedCategory;

                            await _itemFacade.SaveAsync(recievedItem);
                            
                            await Shell.Current.DisplayAlertAsync("Success",
                                $"{recievedItem.Name} was added to {PreviewSelectedCharacter.Name}'s inventory!", "OK");
                        }
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
    
    [RelayCommand]
    private static async Task ShowDiceRollerAsync()
    {
        await Shell.Current.GoToAsync(nameof(DiceRollerPage));
    }
    
    [RelayCommand]
    public async Task ScanQrFromMainAsync()
    {
        await Shell.Current.GoToAsync(nameof(QrScanPage));
    }

    private async Task<bool> ShowItemPreviewAsync(ItemModel item)
    {
        PreviewItem = item;
        PreviewSelectedCharacter = Characters.FirstOrDefault();
        
        var defaultCategories = Enum.GetValues<ItemCategoriesEnum>().Select(e => e.ToString());
        var dbCategories = await _itemFacade.GetAllCategories();
        PreviewAvailableCategories = defaultCategories
            .Union(dbCategories)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToList();
            
        PreviewSelectedCategory = string.IsNullOrWhiteSpace(item.Category) ? "Uncategorized" : item.Category;
        
        var defaultImgs = new[] { "armor.png", "sword.png", "potion.png" };
        PreviewSelectedImage = defaultImgs[0];
        
        PreviewTemplateImages.Clear();
        foreach (var img in defaultImgs)
        {
            PreviewTemplateImages.Add(new TemplateImageItem 
            { 
                ImagePath = img, 
                IsSelected = img == PreviewSelectedImage 
            });
        }
            
        IsItemPreviewVisible = true;
        _previewTcs = new TaskCompletionSource<bool>();
        return await _previewTcs.Task;
    }
    
    [RelayCommand] private void ConfirmItemPreview() { IsItemPreviewVisible = false; _previewTcs?.TrySetResult(true); }
    [RelayCommand] private void CancelItemPreview() { IsItemPreviewVisible = false; _previewTcs?.TrySetResult(false); }

    [RelayCommand] private void OpenCharacterSelect() => IsCharacterSelectVisible = true;
    [RelayCommand] private void CloseCharacterSelect() => IsCharacterSelectVisible = false;

    [RelayCommand] 
    private void SelectCharacter(CharacterModel character) 
    { 
        PreviewSelectedCharacter = character; 
        IsCharacterSelectVisible = false; 
    }
    
    [RelayCommand] private void OpenPreviewCategorySelect() => IsPreviewCategorySelectVisible = true;
    [RelayCommand] private void ClosePreviewCategorySelect() => IsPreviewCategorySelectVisible = false;

    [RelayCommand] 
    private void SelectPreviewCategory(string category) 
    { 
        PreviewSelectedCategory = category; 
        IsPreviewCategorySelectVisible = false; 
    }

    [RelayCommand] 
    private void SelectTemplateImage(TemplateImageItem selected)
    {
        foreach (var img in PreviewTemplateImages)
        {
            img.IsSelected = false;
        }
        selected.IsSelected = true;
        PreviewSelectedImage = selected.ImagePath;
    }
}