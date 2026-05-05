using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Facades;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;
using Java.Lang;
using Exception = System.Exception;

namespace DnD_InventoryManager.ViewModels;

[QueryProperty(nameof(CharacterId), "CharacterId")]
public partial class QrScanViewModel : ViewModelBase
{
    private readonly QrService _qrService;
    private readonly ItemFacade _itemFacade;
    private readonly CharacterFacade _characterFacade;

    [ObservableProperty]
    private int characterId;

    [ObservableProperty]
    private bool isProcessing;

    [ObservableProperty]
    private string statusMessage = "Namierte kameru na QR kód";

    [ObservableProperty]
    private bool isDetecting;

    public QrScanViewModel(QrService qrService, ItemFacade itemFacade, CharacterFacade characterFacade)
    {
        _qrService = qrService;
        _itemFacade = itemFacade;
        _characterFacade = characterFacade;
        Title = "Skenovať QR";
    }

    [RelayCommand]
    public async Task ProcessBarcodeResultAsync(string barcodeText)
    {
        if (IsProcessing) return;
        IsProcessing = true;
        IsDetecting = false;

        StatusMessage = "QR kód nájdený, spracovávam...";

        var result = _qrService.DecodeItem(barcodeText);

        if (!result.IsSuccess || result.Data == null)
        {
            StatusMessage = result.ErrorMessage;
            await Task.Delay(2000);
            IsProcessing = false;
            IsDetecting = true;
            StatusMessage = "Namierte kameru na QR kód";
            return;
        }

        var item = result.Data;
        
        if (CharacterId == 0)
        {
            var characters = await _characterFacade.GetAllAsync();
                
            if (characters == null || !characters.Any())
            {
                await Shell.Current.DisplayAlert("Chyba", "Nemáte vytvorené žiadne postavy, komu by ste to priradili.", "OK");
                StatusMessage = "Namierte kameru na QR kód";
                IsDetecting = true;
                return;
            }
            
            string[] characterNames = characters.Select(c => c.Name).ToArray();
            
            string selectedName = await Shell.Current.DisplayActionSheet(
                $"Kam pridať {item.Name}?", 
                "Zrušiť", 
                null, 
                characterNames);
            
            if (string.IsNullOrEmpty(selectedName) || selectedName == "Zrušiť")
            {
                StatusMessage = "Namierte kameru na QR kód";
                IsDetecting = true;
                return;
            }
            var selectedCharacter = characters.First(c => c.Name == selectedName);
            item.CharacterId = selectedCharacter.Id;
        }
        else
        {
            item.CharacterId = CharacterId;
        }

        await _itemFacade.SaveAsync(item);
        
        try
        {
            Microsoft.Maui.Devices.Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(200));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Vibration failed: {ex.Message}");
        }
        
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.DisplayAlertAsync(
                "Loot acquired!",
                $"Item \"{item.Name}\" bol pridaný do inventára.",
                "OK");
            await Shell.Current.GoToAsync("..");
        });
    }
    
    [RelayCommand]
    public async Task PickFromGalleryAsync()
    {
        try
        {
            IsDetecting = false;
            StatusMessage = "Otváram galériu...";

            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Vyberte QR kód"
            });

            if (photo == null)
            {
                StatusMessage = "Namierte kameru na QR kód";
                IsDetecting = true;
                return;
            }

            StatusMessage = "Spracovávam obrázok...";
            using var stream = await photo.OpenReadAsync();
            string barcodeText = null;

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
                StatusMessage = "❌ V obrázku sa nenašiel žiadny čitateľný QR kód.";
                await Task.Delay(2500);
                
                StatusMessage = "Namierte kameru na QR kód";
                IsDetecting = true;
                return;
            }
            
            await ProcessBarcodeResultAsync(barcodeText);
        }
        catch (Exception ex)
        {
            StatusMessage = "⚠️ Nastala chyba pri načítaní obrázka.";
            Console.WriteLine($"Chyba galérie: {ex.Message}");
            await Task.Delay(2500);
            
            StatusMessage = "Namierte kameru na QR kód";
            IsDetecting = true;
        }
    }
}