using System.Text;
using DnD_InventoryManager.Api;
using DnD_InventoryManager.Entities;
using DnD_InventoryManager.Models;
using Newtonsoft.Json.Linq;
using Riok.Mapperly.Abstractions;

namespace DnD_InventoryManager.Mappers;

[Mapper]
public partial class ItemMapper
{
    public partial ItemModel? ToModel(ItemEntity? item);
    
    [MapperIgnoreSource(nameof(ItemModel.TotalWeight))]
    public partial ItemEntity ToEntity(ItemModel itemModel);
    
    public partial IList<ItemModel> EntitiesToListModels(ICollection<ItemEntity> items);

    public static ItemModel EquipmentModelToItemModel(EquipmentModel equipment)
    {
        var props = equipment.AdditionalProperties;
        var sb = new StringBuilder();

        if (props.TryGetValue("weapon_category", out var wc) && wc != null)
        {
            var range = props.TryGetValue("weapon_range", out var wr) ? $" ({wr})" : "";
            sb.AppendLine($"Equipment Type: {wc}{range}");
        }
        else if (props.TryGetValue("armor_category", out var ac) && ac != null)
        {
            sb.AppendLine($"🛡Armor Type: {ac}");
        }

        if (props.TryGetValue("armor_class", out var acObj) && acObj is JObject armorClass)
        {
            var baseAc = armorClass.Value<int?>("base");
            var dexBonus = armorClass.Value<bool?>("dex_bonus") == true;
            var maxBonus = armorClass.Value<int?>("max_bonus");
            
            var dexStr = dexBonus ? " + Dex" + (maxBonus > 0 ? $" (max {maxBonus})" : "") : "";
            sb.AppendLine($"Armor Class: {baseAc}{dexStr}");
        }
        
        if (props.TryGetValue("stealth_disadvantage", out var stealth) && stealth is bool hasStealthDisadvantage && hasStealthDisadvantage)
        {
            sb.AppendLine($"Stealth: Disadvantage");
        }

        if (props.TryGetValue("damage", out var dmgObj) && dmgObj is JObject damage)
        {
            var dmgDice = damage.Value<string>("damage_dice");
            var dmgType = damage["damage_type"]?.Value<string>("name");
            if (!string.IsNullOrEmpty(dmgDice))
                sb.AppendLine($"Damage: {dmgDice} {dmgType}".Trim());
        }

        if (props.TryGetValue("two_handed_damage", out var thDmgObj) && thDmgObj is JObject twoHandedDamage)
        {
            var dmgDice = twoHandedDamage.Value<string>("damage_dice");
            var dmgType = twoHandedDamage["damage_type"]?.Value<string>("name");
            if (!string.IsNullOrEmpty(dmgDice))
                sb.AppendLine($"Two-Handed: {dmgDice} {dmgType}".Trim());
        }

        if (props.TryGetValue("properties", out var propObj) && propObj is JArray properties && properties.Count > 0)
        {
            var propNames = properties.Select(p => p["name"]?.ToString()).Where(name => !string.IsNullOrEmpty(name));
            sb.AppendLine($"Properties: {string.Join(", ", propNames)}");
        }

        if (props.TryGetValue("cost", out var costObj) && costObj is JObject cost)
        {
            var qty = cost.Value<int?>("quantity");
            var unit = cost.Value<string>("unit");
            sb.AppendLine($"Cost: {qty} {unit}");
        }

        if (props.TryGetValue("desc", out var descObj) && descObj is JArray description && description.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine(string.Join("\n\n", description.Select(d => d.ToString())));
        }

        var equipmentCategory = props.TryGetValue("equipment_category", out var ecObj) && ecObj is JObject ec ? ec : null;
        var name = props.TryGetValue("name", out var n) ? n?.ToString() ?? "" : "";
        var weight = props.TryGetValue("weight", out var w) ? Convert.ToDouble(w) : 0.0;

        var imagePath = "";
        if (props.TryGetValue("image", out var imageObj) && imageObj is not null)
        {
            var imgString = imageObj.ToString();
            if (!string.IsNullOrEmpty(imgString))
            {
                imagePath = IAllItemsApiClient.BaseUrl + imgString;
            }
        }
        
        
        return new ItemModel
        {
            Name = name,
            Weight = weight,
            Quantity = 1,
            Description = sb.ToString().Trim(),
            Category = equipmentCategory?.Value<string>("name") ?? "",
            ImagePath = imagePath
        };
    }

    public static ItemModel MagicItemModelToItemModel(MagicItemModel magicItem)
    {
        var sb = new StringBuilder();


        if (magicItem.Desc != null && magicItem.Desc.Any())
        {
            sb.AppendLine(string.Join("\n\n", magicItem.Desc));
        }

        var imagePath = !string.IsNullOrEmpty(magicItem.Image) 
            ? IAllItemsApiClient.BaseUrl + magicItem.Image 
            : "";

        return new ItemModel
        {
            Name = magicItem.Name,
            Description = sb.ToString().Trim(),
            ImagePath = imagePath,
            Category = magicItem.Equipment_category?.Name ?? "Magic Item",
            Quantity = 1,
            Weight = 0
        };
    }
}