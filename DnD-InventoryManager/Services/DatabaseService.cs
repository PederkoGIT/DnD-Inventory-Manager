using DnD_InventoryManager.Models;
using SQLite;

namespace DnD_InventoryManager.Services;

public class DatabaseService
{
    private const string DbName = "DnDManager.db3";
    private readonly string _dbPath = Path.Combine(FileSystem.AppDataDirectory, DbName); 
    public void Init()
    {
        Task.Run(async () =>
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            await connection.CreateTableAsync<CharacterEntity>();
            await  connection.CreateTableAsync<ItemEntity>();
            await connection.CloseAsync();
        });
    }

    public async Task<T?> GetById<T>(int id) where T: EntityBase, new()
    {
        var connection = new SQLiteAsyncConnection(_dbPath);   
        var entity = await connection!.Table<T>().Where(e => e.Id == id).FirstOrDefaultAsync();
        await connection.CloseAsync();
        return entity;
    }

    public async Task<List<T>> GetAsync<T>() where T: EntityBase, new()
    {
        var connection = new SQLiteAsyncConnection(_dbPath);
        var entities = await connection!.Table<T>().ToListAsync();
        await connection.CloseAsync();
        return entities;
    }

    public async Task SaveAsync<T>(T entity)  where T: EntityBase, new()
    {
        var connection = new SQLiteAsyncConnection(_dbPath);
        if (entity.Id != 0)
        {
            await connection!.UpdateAsync(entity);
        }
        else
        {
            await connection!.InsertAsync(entity);
        }
        await connection.CloseAsync();
    }

    public async Task DeleteAsync<T>(int id) where T: EntityBase, new()
    {
        var connection = new SQLiteAsyncConnection(_dbPath);
        await connection.DeleteAsync<T>(id);
        await connection.CloseAsync();
    }

    public async Task<List<ItemEntity>> GetAllByCharacterId(int characterId)
    {
        var connection = new SQLiteAsyncConnection(_dbPath);
        var entities = await connection!.Table<ItemEntity>().Where(e => e.CharacterId.Equals(characterId)).ToListAsync();
        await connection.CloseAsync();
        return entities;
    }
}