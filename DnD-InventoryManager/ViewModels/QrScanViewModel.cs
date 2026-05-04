using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;
using System.Threading.Tasks;

namespace DnD_InventoryManager.ViewModels;

public partial class QrScanViewModel : ViewModelBase
{
    private readonly QrService _qrService;
    private readonly DatabaseService _databaseService;

    public Action<Character>? OnCharacterScanned { get; set; }

    [ObservableProperty]
    private bool isProcessing;

    [ObservableProperty]
    private string statusMessage = "Namierte kameru na QR kód";

    [ObservableProperty]
    private bool isDetecting;

    public QrScanViewModel(QrService qrService, DatabaseService databaseService)
    {
        _qrService = qrService;
        _databaseService = databaseService;
        Title = "Skenovať QR";
    }

    [RelayCommand]
    public async Task ProcessBarcodeResultAsync(string barcodeText)
    {
        if (IsProcessing) return;
        IsProcessing = true;
        
        IsDetecting = false; 

        StatusMessage = "QR kód nájdený, spracovávam...";

        var result = _qrService.DecodeCharacter(barcodeText);
        
        if (!result.IsSuccess || result.Data == null)
        {
            StatusMessage = result.ErrorMessage;
            await Task.Delay(2000);
            
            IsProcessing = false;
            IsDetecting = true;
            StatusMessage = "Namierte kameru na QR kód";
            return;
        }

        var character = result.Data;
        character.Id = 0;
        await _databaseService.SaveCharacterAsync(character);

        OnCharacterScanned?.Invoke(character);

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.DisplayAlertAsync(
                "Úspech",
                $"Postava \"{character.Name}\" bola pridaná.",
                "OK");
            await Shell.Current.GoToAsync("..");
        });
    }
}