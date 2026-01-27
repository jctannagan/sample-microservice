using Play.Inventory.Service.Dto;

namespace Play.Inventory.Service.Clients
{
    public class CatalogClient : ICatalogClient
    {
        private readonly HttpClient _httpClient;

        public CatalogClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyCollection<CatalogItemDto>> GetCatalogItemsAsync()
        {
            var items = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<CatalogItemDto>>("api/items");
            return items;
        }
    }
}