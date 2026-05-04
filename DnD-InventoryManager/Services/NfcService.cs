using System.Text.Json;
using System.Text.Json.Serialization;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Utils;
using Plugin.NFC;

namespace DnD_InventoryManager.Services;


public class NfcItemModelDto
{
    [JsonPropertyName("n")] public string Name { get; init; } = string.Empty;
    [JsonPropertyName("d")] public string Description { get; init; } = string.Empty;
    [JsonPropertyName("w")] public double Weight { get; init; }
    [JsonPropertyName("q")] public int Quantity { get; init; }
}

public class NfcService
{
    private Action<ItemModel>? _onItemModelReceived;
    private Action<string>? _onError;
    private Action? _onSuccess;
    
    private ItemModel? _itemModelToWrite;
    private bool _isWriting;

    public NfcService()
    {
        CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
        CrossNFC.Current.OnTagDiscovered += Current_OnTagDiscovered;
    }

    public void StartListening(Action<ItemModel> onItemModelReceived, Action<string> onError)
    {
        if (!CrossNFC.Current.IsAvailable) { onError("NFC is not available on this device"); return; }
        
        _isWriting = false;
        _onItemModelReceived = onItemModelReceived;
        _onError = onError;
        
        CrossNFC.Current.StartListening();
    }

    public void StopListening()
    {
        CrossNFC.Current.StopListening();
        _onItemModelReceived = null;
    }

    public void StartWriting(ItemModel itemModel, Action onSuccess, Action<string> onError)
    {
        if (!CrossNFC.Current.IsAvailable) { onError("NFC is not available on this device"); return; }
        
        _isWriting = true;
        _itemModelToWrite = itemModel;
        _onSuccess = onSuccess;
        _onError = onError;
        
        CrossNFC.Current.StartPublishing();
    }
    
    public void StopWriting()
    {
        CrossNFC.Current.StopPublishing();
        _isWriting = false;
    }


    private void Current_OnMessageReceived(ITagInfo tagInfo)
    {
        if (_isWriting) return; 

        try
        {
            var record = tagInfo.Records?.FirstOrDefault();
            
            if (record is not { Payload: not null }) return;
            
            var json = BrotliHelper.DecompressFromBrotli(record.Payload);
                
            var dto = JsonSerializer.Deserialize<NfcItemModelDto>(json);

            if (dto == null) return;
            
            var item = new ItemModel
            {
                Name = dto.Name,
                Description = dto.Description,
                Weight = dto.Weight,
                Quantity = dto.Quantity,
                ImagePath = "dotnet_bot.png"
            };
            
            _onItemModelReceived?.Invoke(item);
        }
        catch (Exception ex)
        {
            _onError?.Invoke($"Couldn't read from tag: {ex.Message}");
        }
    }

    private void Current_OnTagDiscovered(ITagInfo tagInfo, bool format)
    {
        if (!_isWriting || _itemModelToWrite == null) return;

        try
        {
            var dto = new NfcItemModelDto
            {
                Name = _itemModelToWrite.Name,
                Description = _itemModelToWrite.Description,
                Weight = _itemModelToWrite.Weight,
                Quantity = _itemModelToWrite.Quantity
            };
            var json = JsonSerializer.Serialize(dto);
            
            var compressedPayload = BrotliHelper.CompressToBrotli(json);

            var record = new NFCNdefRecord
            {
                TypeFormat = NFCNdefTypeFormat.Mime,
                MimeType = "application/vnd.dnd.brotli",
                Payload = compressedPayload
            };

            tagInfo.Records = [record];
            CrossNFC.Current.PublishMessage(tagInfo);
            
            _onSuccess?.Invoke();
            
            CrossNFC.Current.StopPublishing();
            _isWriting = false;
        }
        catch (Exception ex)
        {
            _onError?.Invoke($"Couldn't write to tag: {ex.Message}");
        }
    }
}