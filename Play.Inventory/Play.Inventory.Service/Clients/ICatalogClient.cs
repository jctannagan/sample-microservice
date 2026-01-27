using Play.Inventory.Service.Dto;

namespace Play.Inventory.Service.Clients
{
    public interface ICatalogClient
    {
        Task<IReadOnlyCollection<CatalogItemDto>> GetCatalogItemsAsync();
    }
}