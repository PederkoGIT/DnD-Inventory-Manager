using DnD_InventoryManager.Mappers;
using DnD_InventoryManager.Models;
using SQLite;

namespace DnD_InventoryManager.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly CharacterMapper _mapper = new();

    private async Task Init()
    {
        if (_database is not null) return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "DnDManager.db3");
        _database = new SQLiteAsyncConnection(dbPath);

        await _database.CreateTableAsync<CharacterEntity>();
    }

    public async Task<Character?> GetCharacterById(int id)
    {
        await Init();
        
        var entity = await _database!.Table<CharacterEntity>().Where(e => e.Id == id).FirstOrDefaultAsync();
        
        if (entity is null) return null;
        
        return _mapper.ToModel(entity);
    }

    public async Task<List<Character>> GetCharactersAsync()
    {
        await Init();
        var entities = await _database!.Table<CharacterEntity>().ToListAsync();

        return entities.Select(e => _mapper.ToModel(e)).ToList();
    }

    public async Task SaveCharacterAsync(Character character)
    {
        await Init();

        var entity = _mapper.ToEntity(character);

        if (entity.Id != 0)
        {
            await _database!.UpdateAsync(entity);
        }
        else
        {
            await _database!.InsertAsync(entity);
            character.Id = entity.Id;
        }
    }

    public async Task DeleteCharacterAsync(int id)
    {
        await Init();
        await _database.DeleteAsync<CharacterEntity>(id);
    }
}