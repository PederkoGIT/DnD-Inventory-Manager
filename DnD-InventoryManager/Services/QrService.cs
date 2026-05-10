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
    public static string EncodeItem(ItemModel item)
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
            return (false, null, "The scanned QR code is empty");

        try
        {
            string jsonText = Decompress(payload);
            
            var dto = JsonSerializer.Deserialize<QrItemDto>(jsonText);
            if (dto == null)
                return (false, null, "Failed to parse data from the QR code");

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
            return (false, null, "The scanned QR code is not in a valid compressed format.");
        }
        catch (JsonException ex)
        {
            return (false, null, $"The scanned code is not a valid D&D item. Details: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, null, $"An unknown error occurred while reading the QR code: {ex.Message}");
        }
    }
    
    private static string Compress(string text)
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

    private static string Decompress(string compressedBase64)
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