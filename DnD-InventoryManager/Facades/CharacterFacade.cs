using DnD_InventoryManager.Entities;
using DnD_InventoryManager.Mappers;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;

namespace DnD_InventoryManager.Facades;

public class CharacterFacade(DatabaseService databaseService, CharacterMapper characterMapper)
{
    public async Task<ICollection<CharacterModel>> GetAllAsync()
    {
        var entities = await databaseService.GetAsync<CharacterEntity>();
        return characterMapper.EntitiesToListModels(entities);
    }

    public async Task<CharacterModel?> GetByIdAsync(int id)
    {
        var entity = await databaseService.GetById<CharacterEntity>(id);
        return characterMapper.ToModel(entity);
    }

    public async Task SaveAsync(CharacterModel characterModel)
    {
        var entity = characterMapper.ToEntity(characterModel);
        await databaseService.SaveAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        await databaseService.DeleteAsync<CharacterEntity>(id);
    }
}