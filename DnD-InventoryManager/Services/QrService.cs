using System.Text.Json;
using System.Text.Json.Serialization;
using DnD_InventoryManager.Models;

namespace DnD_InventoryManager.Services;

public class QrCharacterDto
{
    [JsonPropertyName("n")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("s")] public int Strength { get; set; }
    [JsonPropertyName("z")] public CharacterSizeEnum Size { get; set; }
}

public class QrService
{
    public string EncodeCharacter(Character character)
    {
        var dto = new QrCharacterDto
        {
            Name = character.Name,
            Strength = character.Strength,
            Size = character.Size
        };
        return JsonSerializer.Serialize(dto);
    }
    
    public (bool IsSuccess, Character? Data, string ErrorMessage) DecodeCharacter(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return (false, null, "Naskenovaný QR kód je prázdny.");
        }

        try
        {
            var dto = JsonSerializer.Deserialize<QrCharacterDto>(payload);
            if (dto == null)
            {
                return (false, null, "Dáta z QR kódu sa nepodarilo rozpoznať.");
            }

            var character = new Character
            {
                Name = dto.Name,
                Strength = dto.Strength,
                Size = dto.Size,
                ImagePath = "dotnet_bot.png"
            };
        
            return (true, character, string.Empty);
        }
        catch (JsonException ex)
        {
            return (false, null, $"Naskenovaný kód nie je platná D&D postava. Detail: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, null, $"Vyskytla sa neznáma chyba pri čítaní QR kódu: {ex.Message}");
        }
    }
}