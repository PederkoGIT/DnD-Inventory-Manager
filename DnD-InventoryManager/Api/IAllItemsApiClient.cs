using DnD_InventoryManager.Models;
using Newtonsoft.Json.Linq;
using Refit;
using Xamarin.Google.Crypto.Tink.Subtle;

namespace DnD_InventoryManager.Api;

public interface IAllItemsApiClient
{
    public const string BaseUrl = "https://www.dnd5eapi.co";
    
    [Get("/api/2014/equipment")]
    Task<AllItemsResponse> GetAllEquipmentAsync();

    [Get("/api/2014/magic-items")]
    Task<AllItemsResponse> GetAllMagicItems();
}

public class AllItemsResponse
{
    public int Count {get; set;}
    public List<ItemListApiModel> Results { get; set; } = [];
}