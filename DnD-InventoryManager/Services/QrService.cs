using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DnD_InventoryManager.Models;

namespace DnD_InventoryManager.Services;

public class QrItemDto
{
    [JsonPropertyName("n")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("d")] public string Description { get; set; } = string.Empty;
    [JsonPropertyName("w")] public double Weight { get; set; }
    [JsonPropertyName("q")] public int Quantity { get; set; }
}

public class QrService
{
    public string EncodeItem(ItemModel item)
    {
        var dto = new QrItemDto
        {
            Name = item.Name,
            Description = item.Description,
            Weight = item.Weight,
            Quantity = item.Quantity
        };
        
        string jsonText = JsonSerializer.Serialize(dto);
        
        return Compress(jsonText);
    }

    public (bool IsSuccess, ItemModel? Data, string ErrorMessage) DecodeItem(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return (false, null, "Naskenovaný QR kód je prázdny.");

        try
        {
            string jsonText = Decompress(payload);
            
            var dto = JsonSerializer.Deserialize<QrItemDto>(jsonText);
            if (dto == null)
                return (false, null, "Dáta z QR kódu sa nepodarilo rozpoznať.");

            var item = new ItemModel
            {
                Name = dto.Name,
                Description = dto.Description,
                Weight = dto.Weight,
                Quantity = dto.Quantity,
                ImagePath = "dotnet_bot.png"
            };

            return (true, item, string.Empty);
        }
        catch (FormatException)
        {
            return (false, null, "Naskenovaný QR kód nie je v správnom komprimovanom formáte.");
        }
        catch (JsonException ex)
        {
            return (false, null, $"Naskenovaný kód nie je platný D&D item. Detail: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, null, $"Vyskytla sa neznáma chyba pri čítaní QR kódu: {ex.Message}");
        }
    }
    
    private string Compress(string text)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        
        using var memoryStreamInput = new MemoryStream(bytes);
        using var memoryStreamOutput = new MemoryStream();
        
        using (var gZipStream = new GZipStream(memoryStreamOutput, CompressionLevel.Optimal))
        {
            memoryStreamInput.CopyTo(gZipStream);
        }
        
        return Convert.ToBase64String(memoryStreamOutput.ToArray());
    }

    private string Decompress(string compressedBase64)
    {
        byte[] bytes = Convert.FromBase64String(compressedBase64);
        
        using var memoryStreamInput = new MemoryStream(bytes);
        using var memoryStreamOutput = new MemoryStream();
        
        using (var gZipStream = new GZipStream(memoryStreamInput, CompressionMode.Decompress))
        {
            gZipStream.CopyTo(memoryStreamOutput);
        }
        
        return Encoding.UTF8.GetString(memoryStreamOutput.ToArray());
    }
}