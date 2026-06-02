using DnD_InventoryManager.Entities;
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
            await connection.CreateTableAsync<ItemEntity>();
            await connection.CreateTableAsync<CategoryEntity>();
            
            var categoryCount = await connection.Table<CategoryEntity>().CountAsync();
            if (categoryCount == 0)
            {
                var systemCategories = new List<CategoryEntity>
                {
                    new() { Name = "Equipment", CharacterId = 0, IsSystem = true },
                    new() { Name = "MagicItem", CharacterId = 0, IsSystem = true },
                    new() { Name = "Uncategorized", CharacterId = 0, IsSystem = true }
                };
                
                await connection.InsertAllAsync(systemCategories);
            }
            
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

    public async Task DeleteAllByCharacterId(int characterId)
    {
        var connection = new SQLiteAsyncConnection(_dbPath);
        await connection.ExecuteAsync($"delete from Items where CharacterId={characterId}");
        await connection.CloseAsync();
    }

    public async Task<List<string>> GetCategoriesForCharacterAsync(int characterId)
    {
        var connection = new SQLiteAsyncConnection(_dbPath);
        var categories = await connection.Table<CategoryEntity>()
            .Where(c => c.CharacterId.Equals(characterId) || c.IsSystem)
            .ToListAsync();

        await connection.CloseAsync();
        return categories.Select(c => c.Name).Distinct().ToList();
    }

    public async Task AddCategoryAsync(string name, int characterId)
    {
        var connection = new SQLiteAsyncConnection(_dbPath);
        
        var exists = await connection.Table<CategoryEntity>()
            .Where(c => c.Name == name && (c.CharacterId == characterId || c.CharacterId == 0))
            .FirstOrDefaultAsync();

        if (exists == null)
        {
            await connection.InsertAsync(new CategoryEntity
            {
                Name = name,
                CharacterId = characterId,
                IsSystem = false
            });
        }
        await connection.CloseAsync();
    }
    
    
    public async Task RenameCategoryAsync(string oldCategory, string newCategory, int characterId)
    {
        var connection = new SQLiteAsyncConnection(_dbPath);
    
        var categoryToRename = await connection.Table<CategoryEntity>()
            .Where(c => c.Name == oldCategory && c.CharacterId == characterId && c.IsSystem == false)
            .FirstOrDefaultAsync();

        if (categoryToRename != null)
        {
            categoryToRename.Name = newCategory;
            await connection.UpdateAsync(categoryToRename);
        }

        var itemsToUpdate = await connection.Table<ItemEntity>()
            .Where(e => e.Category == oldCategory && e.CharacterId == characterId)
            .ToListAsync();

        if (itemsToUpdate.Any())
        {
            foreach (var item in itemsToUpdate)
            {
                item.Category = newCategory;
            }
            await connection.UpdateAllAsync(itemsToUpdate);
        }
    
        await connection.CloseAsync();
    }

    public async Task DeleteCategoryAndReassignAsync(string oldCategory, string newCategory, int characterId)
    {
        var connection = new SQLiteAsyncConnection(_dbPath);
        
        var categoryToDelete = await connection.Table<CategoryEntity>()
            .Where(c => c.Name == oldCategory && c.CharacterId == characterId && c.IsSystem == false)
            .FirstOrDefaultAsync();

        if (categoryToDelete != null)
        {
            await connection.DeleteAsync(categoryToDelete);
        }
        
        var itemsToUpdate = await connection.Table<ItemEntity>()
            .Where(i => i.Category == oldCategory && i.CharacterId == characterId)
            .ToListAsync();

        if (itemsToUpdate.Any())
        {
            foreach (var item in itemsToUpdate)
            {
                item.Category = newCategory;
            }
            await connection.UpdateAllAsync(itemsToUpdate);
        }
        
        await connection.CloseAsync();
    }
    
    
}