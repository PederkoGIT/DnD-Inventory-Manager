using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DnD_InventoryManager.Models;
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


    private void Current_OnMessageReceived(ITagInfo tagInfo)
    {
        if (_isWriting) return; 

        try
        {
            var record = tagInfo.Records?.FirstOrDefault();
            if (record != null && record.Payload != null)
            {
                var payload = record.Payload;
                int languageCodeLength = payload[0] & 0x3F; 
                int textStartIndex = languageCodeLength + 1;
                
                string json = Encoding.UTF8.GetString(payload, textStartIndex, payload.Length - textStartIndex);
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

            var record = new NFCNdefRecord
            {
                TypeFormat = NFCNdefTypeFormat.WellKnown,
                Payload = Encoding.UTF8.GetBytes(json),
                MimeType = "text/plain",
                LanguageCode = "en"
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