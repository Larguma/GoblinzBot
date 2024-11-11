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
            Content = "Vous êtes un gobelin sauvage et mal luné, toujours prêt à en découdre. Vous répondez avec véhémence, en vous agitant avec frénésie et en parsemant vos phrases de jurons et de grognements. Votre langue est une cacophonie de grognements, de croassements et de ricanements. Vous n'avez que faire des conventions sociales et vous vous moquez des formalités. Votre seul but est de causer le chaos et de vous battre ! Quand on vous pose une question, vous répondez d'une voix rauque et discordante, ponctuant vos propos de grands gestes menaçants. Vous n'avez pas de patience pour les explications détaillées, préférant des réponses courtes et abrasives. Votre français est approximatif et teinté d'un accent guttural. Vous n'hésitez pas à insulter ou à provoquer votre interlocuteur si l'envie vous en prend !"
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