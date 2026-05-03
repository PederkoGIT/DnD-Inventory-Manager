using System.Text.Json;
using System.Text.Json.Serialization;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Utils;
using Plugin.NFC;

namespace DnD_InventoryManager.Services;

public class NfcCharacterModelDto
{
    [JsonPropertyName("n")] public string Name { get; init; } = string.Empty;
    [JsonPropertyName("s")] public int Strength { get; init; }
    [JsonPropertyName("z")] public CharacterSizeEnum Size { get; init; }
}

public class NfcService
{
    private Action<CharacterModel>? _onCharacterModelReceived;
    private Action<string>? _onError;
    private Action? _onSuccess;
    
    private CharacterModel? _CharacterModelToWrite;
    private bool _isWriting;

    public NfcService()
    {
        CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
        CrossNFC.Current.OnTagDiscovered += Current_OnTagDiscovered;
    }

    public void StartListening(Action<CharacterModel> onCharacterModelReceived, Action<string> onError)
    {
        if (!CrossNFC.Current.IsAvailable) { onError("NFC is not available on this device"); return; }
        
        _isWriting = false;
        _onCharacterModelReceived = onCharacterModelReceived;
        _onError = onError;
        
        CrossNFC.Current.StartListening();
    }

    public void StopListening()
    {
        CrossNFC.Current.StopListening();
        _onCharacterModelReceived = null;
    }

    public void StartWriting(CharacterModel CharacterModel, Action onSuccess, Action<string> onError)
    {
        if (!CrossNFC.Current.IsAvailable) { onError("NFC is not available on this device"); return; }
        
        _isWriting = true;
        _CharacterModelToWrite = CharacterModel;
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
                
            var dto = JsonSerializer.Deserialize<NfcCharacterModelDto>(json);

            if (dto == null) return;
            
            var character = new CharacterModel
            {
                Name = dto.Name,
                Strength = dto.Strength,
                Size = dto.Size,
                ImagePath = "dotnet_bot.png" 
            };
            
            _onCharacterModelReceived?.Invoke(character);
        }
        catch (Exception ex)
        {
            _onError?.Invoke($"Couldn't read from tag: {ex.Message}");
        }
    }

    private void Current_OnTagDiscovered(ITagInfo tagInfo, bool format)
    {
        if (!_isWriting || _CharacterModelToWrite == null) return;

        try
        {
            var dto = new NfcCharacterModelDto
            {
                Name = _CharacterModelToWrite.Name,
                Strength = _CharacterModelToWrite.Strength,
                Size = _CharacterModelToWrite.Size
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