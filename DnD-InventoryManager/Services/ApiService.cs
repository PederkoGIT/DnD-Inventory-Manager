using DnD_InventoryManager.Api;
using DnD_InventoryManager.Models;

namespace DnD_InventoryManager.Services;

public class ApiService
{
    private readonly EquipmentClient _equipmentClient = new(new HttpClient());
    
    public async Task<ItemModel> GetEquipmentFromApiAsync(string index)
    {
        var equipment = await _equipmentClient.EquipmentAsync(index);
        return new ItemModel
        {
            Name = equipment.AdditionalProperties["name"] as string ?? "",
            Weight = equipment.AdditionalProperties["weight"] as double? ?? 0,
        };
    }

    public async Task<ItemModel> GetMagicItemFromApiAsync(string index)
    {
        var magicItem = await _equipmentClient.MagicItemsAsync(index);
        return new ItemModel
        {
            Name = magicItem.Name,
            Description = string.Join(", ", magicItem.Desc)
        };
    }
}