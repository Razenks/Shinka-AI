using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ChatBot.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        //O construtor recebe o HttpClient para fazer as chamadas e a Configuration para ler nossa API key
        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"]; //Pega a chave do appsettings.json
        }

        public async Task<string> GenerateContentAsync(string prompt)
        {
            //Este é o endereço da API do Gemini para o modelo gemini-pro
            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            //O Gemini espera um JSON com uma estrutura específica. Nós a montamos aqui.
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt}
                        }
                    }
                }
            };

            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                // Enviamos a requisição (POST) para a API do Gemini
                var response = await _httpClient.PostAsync(apiUrl, httpContent);

                // Verificamos se a resposta foi bem-sucedida
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();

                    // A resposta do Gemini também é um JSON complexo. Nós Extraímos apenas o texto gerado    
                    dynamic responseObject = JsonConvert.DeserializeObject(responseBody);
                    string generatedText = responseObject.candidates[0].content.parts[0].text;

                    return generatedText;
                }
                else
                {
                    // Se deu erro, lemos a mensagem de erro para nos ajudar a depurar depois
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Erro ao chamar a API do Gemini: {response.ReasonPhrase} | {errorContent}";
                }
            }
            catch (Exception ex)
            {
                // Capturamos qualquer exceção que possa ocorrer durante a chamada HTTP
                return $"Ocorreu uma exceção: {ex.Message}";
            }
        }
    }
}