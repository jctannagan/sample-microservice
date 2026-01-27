using Play.Inventory.Service.Dto;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service
{
    public static class Extensions
    {
        public static InventoryItemDto AsDto(this InventoryItem inventoryItem, string name, string description)
        {
            return new InventoryItemDto(inventoryItem.CatalogItemId, inventoryItem.Quantity, inventoryItem.AcquiredDate, name, description);
        }
    }
}