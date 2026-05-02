using DnD_InventoryManager.Entities;
using DnD_InventoryManager.Mappers;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;

namespace DnD_InventoryManager.Facades;

public class ItemFacade(DatabaseService databaseService, ItemMapper itemMapper)
{
    public async Task<ICollection<ItemModel>> GetAllByCharacterIdAsync(int id)
    {
        var entities = await databaseService.GetAllByCharacterId(id);
        return itemMapper.EntitiesToListModels(entities);
    }

    public async Task<ItemModel?> GetByIdAsync(int id)
    {
        var entity = await databaseService.GetById<ItemEntity>(id);
        return itemMapper.ToModel(entity);
    }

    public async Task SaveAsync(ItemModel itemModel)
    {
        var entity = itemMapper.ToEntity(itemModel);
        await databaseService.SaveAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        await databaseService.DeleteAsync<ItemEntity>(id);
    }
}