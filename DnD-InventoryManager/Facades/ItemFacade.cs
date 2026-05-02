using DnD_InventoryManager.Entities;
using DnD_InventoryManager.Mappers;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;

namespace DnD_InventoryManager.Facades;

public class ItemFacade(DatabaseService databaseService, ItemMapper itemMapper)
{
    public async Task<ICollection<Item>> GetAllByCharacterIdAsync(int id)
    {
        var entities = await databaseService.GetAllByCharacterId(id);
        return itemMapper.EntitiesToListModels(entities);
    }

    public async Task<Item?> GetByIdAsync(int id)
    {
        var entity = await databaseService.GetById<ItemEntity>(id);
        return itemMapper.ToModel(entity);
    }

    public async Task SaveAsync(Item item)
    {
        var entity = itemMapper.ToEntity(item);
        await databaseService.SaveAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        await databaseService.DeleteAsync<ItemEntity>(id);
    }
}