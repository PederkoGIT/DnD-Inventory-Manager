using DnD_InventoryManager.Mappers;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;

namespace DnD_InventoryManager.Facades;

public class CharacterFacade(DatabaseService databaseService, CharacterMapper characterMapper)
{
    public async Task<List<Character>> GetAllAsync()
    {
        var entities = await databaseService.GetAsync<CharacterEntity>();
        return entities.Select(e => characterMapper.ToModel(e)).ToList();
    }

    public async Task<Character?> GetByIdAsync(int id)
    {
        var entity = await databaseService.GetById<CharacterEntity>(id);
        return characterMapper.ToModel(entity);
    }

    public async Task<Character> SaveAsync(Character character)
    {
        var entity = characterMapper.ToEntity(character);
        await databaseService.SaveAsync(entity);
        return characterMapper.ToModel(entity);
    }

    public async Task DeleteAsync(int id)
    {
        await databaseService.DeleteAsync<CharacterEntity>(id);
    }
}