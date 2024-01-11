using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Item
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    [BsonElement("Name")]
    public string Lesson { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public DateTime End { get; set; } = DateTime.MinValue;

    public bool IsExam { get; set; } = false;

    public string GuildId { get; set; } = string.Empty;

    public int Color { get; set; } = 1;
}
