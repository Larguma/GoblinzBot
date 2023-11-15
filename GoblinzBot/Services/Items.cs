using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

public class ItemsService
{
  private readonly IMongoCollection<Item> _itemsCollection;

  public ItemsService(IOptions<DatabaseSettings> databaseSettings)
  {
    MongoClient mongoClient = new(databaseSettings.Value.ConnectionString);

    IMongoDatabase mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);

    _itemsCollection = mongoDatabase.GetCollection<Item>(databaseSettings.Value.ItemsCollectionName);
  }

  public async Task<List<Item>> GetAsync() =>
      await _itemsCollection.Find(_ => true).ToListAsync();

  public async Task<Item?> GetAsync(ObjectId id) =>
      await _itemsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

  public async Task CreateAsync(Item newItem) =>
    await _itemsCollection.InsertOneAsync(newItem);

  public async Task UpdateAsync(ObjectId id, Item updatedItem) =>
      await _itemsCollection.ReplaceOneAsync(x => x.Id == id, updatedItem);

  public async Task RemoveAsync(ObjectId id) =>
      await _itemsCollection.DeleteOneAsync(x => x.Id == id);
}
