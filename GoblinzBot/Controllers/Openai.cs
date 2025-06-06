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

  public async Task<string?> GetResponseAsync(string content, string query)
  {
    List<OpenaiCounter> counters = await _openaiService.GetAsync();
    if (counters.Count == 0) CreateBaseCounter();

    OpenaiCounter? counter = counters.FirstOrDefault();
    if (counter?.LastUsed < DateTime.Now.AddSeconds(-10))
    {
      counter.LastUsed = DateTime.Now;
      await _openaiService.UpdateAsync(counter);

      OpenaiQuery openaiQuery = new()
      {
        Model = "gpt-4o-mini",
        Messages = {
          new() {
            Role = "system",
            Content = content
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
      return "You find Goblinz blacked out on the floor, he's not responding";
    }
    
    var timeLeft = counter?.LastUsed.AddSeconds(10) - DateTime.Now;
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