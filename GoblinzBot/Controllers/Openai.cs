using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

public class OpenaiController
{
  static readonly HttpClient client = new();
  private readonly OpenaiService _openaiService;

  public OpenaiController(string token, IServiceProvider serviceProvider)
  {
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    _openaiService = serviceProvider.GetRequiredService<OpenaiService>();
  }

  public async Task<string?> GetResponseAsync(string query)
  {
    List<OpenaiCounter> counters = await _openaiService.GetAsync();
    if (counters.Count == 0) CreateBaseCounter();

    OpenaiCounter? counter = counters.FirstOrDefault();
    if (counter?.LastUsed < DateTime.Now.AddMinutes(-2))
    {
      counter.LastUsed = DateTime.Now;
      await _openaiService.UpdateAsync(counter);

      OpenaiQuery openaiQuery = new()
      {
        Model = "gpt-3.5-turbo",
        Messages = {
          new() {
            Role = "system",
            Content = "You are a goblin, you like to gibberish, you are aggressive, mad and you like to fight. You punctuate your answer with gestures. You answer only in french and like to keep your answer quick"
          },
          new() {
            Role = "user",
            Content = query
          }
        }
      };

      HttpResponseMessage response = await client.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", openaiQuery);
      if (response.IsSuccessStatusCode)
      {
        OpenaiResponse? openaiResponse = await response.Content.ReadFromJsonAsync<OpenaiResponse>();
        if (openaiResponse != null)
        {
          return openaiResponse.Choices[0]?.Message?.Content;
        }
      }
    }

    var timeLeft = counter?.LastUsed.AddMinutes(2) - DateTime.Now;
    return "Goblinz is still sleeping for " + (int)(timeLeft?.TotalSeconds ?? 0) + " seconds";
  }

  private async void CreateBaseCounter()
  {
    await _openaiService.CreateAsync(new()
    {
      LastUsed = DateTime.Now.AddMinutes(-10)
    });
  }
}