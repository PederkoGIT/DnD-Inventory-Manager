using DnD_InventoryManager.Api;
using DnD_InventoryManager.Models;

namespace DnD_InventoryManager.Services;

public class ItemService
{
    private readonly EquipmentClient _equipmentClient = new(new HttpClient());
    
    public async Task<Item> GetEquipmentFromApiAsync(string index)
    {
        var equipment = await _equipmentClient.EquipmentAsync(index);
        return new Item
        {
            Name = equipment.AdditionalProperties["name"] as string ?? "",
            Weight = equipment.AdditionalProperties["weight"] as double? ?? 0,
        };
    }

    public async Task<Item> GetMagicItemFromApiAsync(string index)
    {
        var magicItem = await _equipmentClient.MagicItemsAsync(index);
        return new Item
        {
            Name = magicItem.Name,
            Description = string.Join(", ", magicItem.Desc)
        };
    }
}