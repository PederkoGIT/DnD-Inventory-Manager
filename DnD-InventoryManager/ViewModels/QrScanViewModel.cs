using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Facades;
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
        
        if (CharacterId == 0)
        {
            var characters = await _characterFacade.GetAllAsync();
                
            if (characters.Count == 0)
            {
                await Shell.Current.DisplayAlertAsync("Error", "You don't have any characters created to assign this to.", "OK");
                StartScanning();
                return;
            }
            
            var characterNames = characters.Select(c => c.Name).ToArray();
            
            var selectedName = await Shell.Current.DisplayActionSheetAsync(
                $"Where do you want to add {item.Name}?", 
                "Cancel", 
                null, 
                characterNames);
            
            if (string.IsNullOrEmpty(selectedName) || selectedName == "Cancel")
            {
                StartScanning();
                return;
            }
            var selectedCharacter = characters.First(c => c.Name == selectedName);
            item.CharacterId = selectedCharacter.Id;
        }
        else
        {
            item.CharacterId = CharacterId;
        }

        var allCategories = await _itemFacade.GetAllCategories();
        var selectedCategory = await Shell.Current.DisplayActionSheetAsync(
            $"Choose category for the new item {item.Name}?", 
            "Cancel", 
            null, 
            allCategories.ToArray()
            );
            
        if (string.IsNullOrEmpty(selectedCategory) || selectedCategory == "Cancel")
        {
            StartScanning();
            return;
        }
        item.Category = selectedCategory;
        
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