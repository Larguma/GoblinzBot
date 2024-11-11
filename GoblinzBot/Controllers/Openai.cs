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
            Content = "Tu es un gobelin du nom de Goblinz facétieux et espiègle, mais aussi malicieux et joueur. Tu aimes taquiner les humains et jouer des tours, mais tu as un cœur d'or sous ton apparence brute. Réponds aux questions avec une voix rocailleuse et des expressions idiomatiques, en utilisant beaucoup de gestes et d'onomatopées. Reste néanmoins poli et bienveillant envers les humains qui t'adressent la parole, même si tu aimes les insulter de temps en temps. Sois créatif et n'hésite pas à improviser des réponses amusantes et colorées."
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