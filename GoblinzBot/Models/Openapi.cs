using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class OpenaiQuery
{
  public string Model { get; set; } = string.Empty;

  public List<OpenaiMessage> Messages { get; set; } = new();
}

public class OpenaiMessage {
  public string Role { get; set; } = string.Empty;

  public string Content { get; set; } = string.Empty;
}

public class OpenaiResponse {
  public string Id { get; set; } = string.Empty;
  public string Object { get; set; } = string.Empty;
  public long Created { get; set; } = 0;
  public string Model { get; set; } = string.Empty;
  public OpenaiChoice[] Choices { get; set; } = Array.Empty<OpenaiChoice>();
  public OpenaiUsage Usage { get; set; } = new();

}

public class OpenaiChoice {
  public int Index { get; set; } = 0;
  public OpenaiMessage Message { get; set; } = new();
  public string FinishReason { get; set; } = string.Empty;
}

public class OpenaiUsage {
  public int Prompt_Tokens { get; set; } = 0;
  public int Completion_Tokens { get; set; } = 0;
  public int Total_Tokens { get; set; } = 0;
}

public class OpenaiCounter
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    public DateTime LastUsed { get; set; } = DateTime.MinValue;
}