using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Utils;
using Plugin.NFC;

namespace DnD_InventoryManager.Services;

public class NfcCharacterDto
{
    [JsonPropertyName("n")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("s")] public int Strength { get; set; }
    [JsonPropertyName("z")] public CharacterSizeEnum Size { get; set; }
}

public class NfcService
{
    private Action<Character>? _onCharacterReceived;
    private Action<string>? _onError;
    private Action? _onSuccess;
    
    private Character? _characterToWrite;
    private bool _isWriting;

    public NfcService()
    {
        CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
        CrossNFC.Current.OnTagDiscovered += Current_OnTagDiscovered;
    }

    public void StartListening(Action<Character> onCharacterReceived, Action<string> onError)
    {
        if (!CrossNFC.Current.IsAvailable) { onError("NFC is not available on this device"); return; }
        
        _isWriting = false;
        _onCharacterReceived = onCharacterReceived;
        _onError = onError;
        
        CrossNFC.Current.StartListening();
    }

    public void StopListening()
    {
        CrossNFC.Current.StopListening();
        _onCharacterReceived = null;
    }

    public void StartWriting(Character character, Action onSuccess, Action<string> onError)
    {
        if (!CrossNFC.Current.IsAvailable) { onError("NFC is not available on this device"); return; }
        
        _isWriting = true;
        _characterToWrite = character;
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
            if (record != null && record.Payload != null)
            {
                string json = BrotliHelper.DecompressFromBrotli(record.Payload);
                
                var dto = JsonSerializer.Deserialize<NfcCharacterDto>(json);

                if (dto != null)
                {
                    var character = new Character
                    {
                        Name = dto.Name,
                        Strength = dto.Strength,
                        Size = dto.Size,
                        ImagePath = "dotnet_bot.png" 
                    };
                    _onCharacterReceived?.Invoke(character);
                }
            }
        }
        catch (Exception ex)
        {
            _onError?.Invoke($"Couldnt read from tag: {ex.Message}");
        }
    }

    private void Current_OnTagDiscovered(ITagInfo tagInfo, bool format)
    {
        if (!_isWriting || _characterToWrite == null) return;

        try
        {
            var dto = new NfcCharacterDto
            {
                Name = _characterToWrite.Name,
                Strength = _characterToWrite.Strength,
                Size = _characterToWrite.Size
            };
            string json = JsonSerializer.Serialize(dto);
            
            byte[] compressedPayload = BrotliHelper.CompressToBrotli(json);

            var record = new NFCNdefRecord
            {
                TypeFormat = NFCNdefTypeFormat.Mime,
                MimeType = "application/vnd.dnd.brotli",
                Payload = compressedPayload
            };

            tagInfo.Records = new[] { record };
            CrossNFC.Current.PublishMessage(tagInfo);
            
            _onSuccess?.Invoke();
            
            CrossNFC.Current.StopPublishing();
            _isWriting = false;
        }
        catch (Exception ex)
        {
            _onError?.Invoke($"Couldnt write to tag: {ex.Message}");
        }
    }
}