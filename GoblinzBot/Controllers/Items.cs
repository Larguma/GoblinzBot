using MongoDB.Bson;

public class ItemsController
{
  private readonly ItemsService _itemsService;

  public ItemsController(ItemsService itemsService) =>
    _itemsService = itemsService;

  public async Task<List<Item>> Index() =>
    await _itemsService.GetAsync();

  public async Task<Item?> Details(ObjectId id) =>
    await _itemsService.GetAsync(id);

  public async Task Create(Item newItem) =>
    await _itemsService.CreateAsync(newItem);

  public async Task Delete(ObjectId id) =>
    await _itemsService.RemoveAsync(id);

  public async Task Update(ObjectId id, Item updatedItem) =>
    await _itemsService.UpdateAsync(id, updatedItem);
}
