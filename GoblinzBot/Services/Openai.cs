using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class OpenaiService
{
  private readonly IMongoCollection<OpenaiCounter> _openaiCollection;

  public OpenaiService(IOptions<DatabaseSettings> databaseSettings)
  {
    MongoClient mongoClient = new(databaseSettings.Value.ConnectionString);

    IMongoDatabase mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);

    _openaiCollection = mongoDatabase.GetCollection<OpenaiCounter>(databaseSettings.Value.OpenaiCollectionName);
  }

  public async Task<List<OpenaiCounter>> GetAsync() =>
    await _openaiCollection.Find(_ => true).ToListAsync();
  public async Task UpdateAsync(OpenaiCounter updatedItem) =>
    await _openaiCollection.ReplaceOneAsync(x => x.Id == updatedItem.Id, updatedItem);

  public async Task CreateAsync(OpenaiCounter newItem) =>
    await _openaiCollection.InsertOneAsync(newItem);
}
