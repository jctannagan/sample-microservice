using MongoDB.Driver;
using Play.Catalog.Service.Entities;

namespace Play.Catalog.Service.Repositories
{
    public class ItemsRepository
    {
        // This is similar to defining Tables on a DBContext class in EFCore
        private const string CollectionName = "items";
        private readonly IMongoCollection<Item> dbCollection;
        private readonly FilterDefinitionBuilder<Item> filterBuilder = Builders<Item>.Filter;

        public ItemsRepository()
        {
            var mongoClient = new MongoClient();
        }
    }
}