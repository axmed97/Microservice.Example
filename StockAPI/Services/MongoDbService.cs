using MongoDB.Driver;

namespace StockAPI.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _mongoDatabase;
        public MongoDbService(IConfiguration configuration)
        {
            MongoClient client = new(configuration.GetConnectionString("Default"));
            _mongoDatabase = client.GetDatabase("StockDb");
        }

        public IMongoCollection<T> GetCollection<T>() =>
            _mongoDatabase.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
    }
}
