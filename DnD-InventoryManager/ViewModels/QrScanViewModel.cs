using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnD_InventoryManager.Facades;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;

namespace DnD_InventoryManager.ViewModels;

[QueryProperty(nameof(CharacterId), "CharacterId")]
public partial class QrScanViewModel : ViewModelBase
{
    private readonly QrService _qrService;
    private readonly ItemFacade _itemFacade;

    [ObservableProperty]
    private int characterId;

    [ObservableProperty]
    private bool isProcessing;

    [ObservableProperty]
    private string statusMessage = "Namierte kameru na QR kód";

    [ObservableProperty]
    private bool isDetecting;

    public QrScanViewModel(QrService qrService, ItemFacade itemFacade)
    {
        _qrService = qrService;
        _itemFacade = itemFacade;
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

        var character = result.Data;
        character.Id = 0;
        await _databaseService.SaveCharacterAsync(character);

        OnCharacterScanned?.Invoke(character);

        var item = result.Data;
        item.CharacterId = CharacterId;
        await _itemFacade.SaveAsync(item);
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.DisplayAlertAsync(
                "Loot acquired!",
                $"Item \"{item.Name}\" bol pridaný do inventára.",
                "OK");
            await Shell.Current.GoToAsync("..");
        });
    }
}