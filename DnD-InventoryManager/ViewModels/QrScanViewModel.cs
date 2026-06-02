using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Facades;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;
using Exception = System.Exception;

namespace DnD_InventoryManager.ViewModels;

[QueryProperty(nameof(CharacterId), "CharacterId")]
public partial class QrScanViewModel : ViewModelBase
{
    private readonly QrService _qrService;
    private readonly ItemFacade _itemFacade;
    private readonly CharacterFacade _characterFacade;

    [ObservableProperty]
    private int _characterId;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string _statusMessage = "Point the camera at the QR code";

    [ObservableProperty]
    private bool _isDetecting;
    
    public ObservableCollection<CharacterModel> Characters { get; } = [];
    
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

    public QrScanViewModel(QrService qrService, ItemFacade itemFacade, CharacterFacade characterFacade)
    {
        _qrService = qrService;
        _itemFacade = itemFacade;
        _characterFacade = characterFacade;
        Title = "Scan QR";
    }

    [RelayCommand]
    public async Task ProcessBarcodeResultAsync(string barcodeText)
    {
        if (IsProcessing) return;
        IsProcessing = true;
        IsDetecting = false;

        StatusMessage = "QR code was found, processing...";

        var result = _qrService.DecodeItem(barcodeText);

        if (!result.IsSuccess || result.Data == null)
        {
            StatusMessage = result.ErrorMessage;
            await Task.Delay(2000);
            IsProcessing = false;
            StartScanning();
            return;
        }

        var item = result.Data;

        var confirm = await ShowItemPreviewAsync(item);

        if (confirm && PreviewSelectedCharacter != null)
        {
            item.CharacterId = PreviewSelectedCharacter.Id;
            item.ImagePath = PreviewSelectedImage;
            item.Category = PreviewSelectedCategory;

            await _itemFacade.SaveAsync(item);
            
            try
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(200));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Vibration failed: {ex.Message}");
            }
            
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.DisplayAlertAsync(
                    "Loot acquired!",
                    $"Item \"{item.Name}\" was added to inventory.",
                    "OK");
                await Shell.Current.GoToAsync("..");
            });
        }
        else
        {
            IsProcessing = false;
            StartScanning();
        }
    }
    
    private async Task<bool> ShowItemPreviewAsync(ItemModel item)
    {
        var chars = await _characterFacade.GetAllAsync();
        Characters.Clear();
        foreach (var c in chars)
        {
            Characters.Add(c);
        }

        if (Characters.Count == 0)
        {
            await Shell.Current.DisplayAlertAsync("Error", "You don't have any characters created to assign this to.", "OK");
            return false;
        }

        PreviewItem = item;
        
        PreviewSelectedCharacter = CharacterId != 0 
            ? Characters.FirstOrDefault(c => c.Id == CharacterId) ?? Characters.FirstOrDefault()
            : Characters.FirstOrDefault();

        var defaultCategories = Enum.GetValues<ItemCategoriesEnum>().Select(e => e.ToString());
        var dbCategories = PreviewSelectedCharacter != null 
            ? await _itemFacade.GetCategoriesForCharacterAsync(PreviewSelectedCharacter.Id) 
            : [];
            
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
    private async Task SelectCharacterAsync(CharacterModel character) 
    { 
        PreviewSelectedCharacter = character; 
        IsCharacterSelectVisible = false; 
        
        var dbCategories = await _itemFacade.GetCategoriesForCharacterAsync(character.Id);
        
        PreviewAvailableCategories = Enum.GetValues<ItemCategoriesEnum>().Select(e => e.ToString())
            .Union(dbCategories)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToList();
            
        if (!PreviewAvailableCategories.Contains(PreviewSelectedCategory))
        {
            PreviewSelectedCategory = "Uncategorized";
        }
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
    
    [RelayCommand]
    private async Task PickFromGalleryAsync()
    {
        try
        {
            IsDetecting = false;
            StatusMessage = "Opening gallery...";

            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select a photo with QR code"
            });

            if (photo == null)
            {
                StartScanning();
                return;
            }

            StatusMessage = "Processing photo...";
            using var stream = await photo.OpenReadAsync();
            string? barcodeText = null;

#if ANDROID
            var bitmap = Android.Graphics.BitmapFactory.DecodeStream(stream);
            if (bitmap != null)
            {
                int[] pixels = new int[bitmap.Width * bitmap.Height];
                bitmap.GetPixels(pixels, 0, bitmap.Width, 0, 0, bitmap.Width, bitmap.Height);

                byte[] rgbBytes = new byte[pixels.Length * 3];
                for (int i = 0; i < pixels.Length; i++)
                {
                    int color = pixels[i];
                    rgbBytes[i * 3]     = (byte)((color >> 16) & 0xFF); // R
                    rgbBytes[i * 3 + 1] = (byte)((color >> 8) & 0xFF);  // G
                    rgbBytes[i * 3 + 2] = (byte)(color & 0xFF);         // B
                }
                
                var source = new ZXing.RGBLuminanceSource(rgbBytes, bitmap.Width, bitmap.Height);
                var reader = new ZXing.BarcodeReaderGeneric
                {
                    Options = new ZXing.Common.DecodingOptions
                    {
                        PossibleFormats = new List<ZXing.BarcodeFormat> { ZXing.BarcodeFormat.QR_CODE },
                        TryHarder = true 
                    }
                };

                var result = reader.Decode(source);
                barcodeText = result?.Text;
            }
#endif

            if (string.IsNullOrEmpty(barcodeText))
            {
                StatusMessage = "No QR code found in image.";
                await Task.Delay(2500);
                
                StartScanning();
                return;
            }
            
            await ProcessBarcodeResultAsync(barcodeText);
        }
        catch (Exception ex)
        {
            StatusMessage = "Loading image error.";
            Console.WriteLine($"Gallery error: {ex.Message}");
            await Task.Delay(2500);
            
            StartScanning();
        }
    }

    private void StartScanning()
    {
        StatusMessage = "Point the camera at the QR code";
        IsDetecting = true;
    }
}