using DnD_InventoryManager.Api;
using DnD_InventoryManager.Entities;
using DnD_InventoryManager.Mappers;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;
using Refit;

namespace DnD_InventoryManager.Facades;

public class ItemFacade(
    DatabaseService databaseService,
    ItemMapper itemMapper,
    EquipmentClient equipmentClient
    )
{
    private readonly IAllItemsApiClient _dndApi = RestService.For<IAllItemsApiClient>(IAllItemsApiClient.BaseUrl);
    
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

    public async Task<ItemModel> GetFromEquipmentApi(string index)
    {
        var resp = await equipmentClient.EquipmentAsync(index.Replace(" ", "-").ToLower());
        return ItemMapper.EquipmentModelToItemModel(resp);
    }

    public async Task<ItemModel> GetFromMagicItemApi(string index)
    {
        var resp = await equipmentClient.MagicItemsAsync(index.Replace(" ", "-").ToLower());
        return ItemMapper.MagicItemModelToItemModel(resp);
    }

    public async Task<List<ItemListApiModel>> GetAllEquipmentApiAsync()
    {
        var resp = await _dndApi.GetAllEquipmentAsync();
        return resp.Results;
    }

    public async Task<List<ItemListApiModel>> GetAllMagicItemsAsync()
    {
        var resp = await _dndApi.GetAllMagicItems();
        return resp.Results;
    }
}