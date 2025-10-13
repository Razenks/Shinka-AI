using System.Text;
using Newtonsoft.Json;
using ChatBot.Models;

namespace ChatBot.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("API Key do Gemini não encontrada.");
        }

        public async Task<string> GenerateContentAsync(string prompt)
        {
            var tool = new Tool
            {
                FunctionDeclarations = new List<FunctionDeclaration> {
                    new FunctionDeclaration {
                        Name = "get_current_datetime",
                        Description = "Obtém a data e a hora atuais.",
                        Parameters = new { type = "OBJECT", properties = new {} }
                    }
                }
            };
            var initialRequest = new GeminiRequest
{
    Contents = new List<Content>
    {
        // 1. INSTRUÇÃO DE SISTEMA BEM DETALHADA:
        new Content
        {
            Role = "user",
            Parts = new List<Part> { new Part { Text = @"
                Você é um chatbot 'Shinka Bot', um experiente profissional de TI com 15 anos de vivência no setor.
                Sua especialidade é desenvolvimento de software, Infra (geral) e segurança da informação.
                Seu tom deve ser sempre **didático, profissional, paciente e extremamente detalhado**.
                Você ama explicar conceitos complexos de forma que qualquer iniciante possa entender, usando analogias do dia a dia quando possível, mas sem perder o rigor técnico.

                **Sua missão:**
                1.  **Educar:** Ajudar usuários a entenderem problemas e soluções técnicas.
                2.  **Solucionar:** Oferecer passos claros e práticos para resolver questões de TI.
                3.  **Orientar:** Sugerir as melhores práticas e caminhos de aprendizado.

                **Comportamentos Específicos:**
                * Comece suas respostas com uma saudação que reforce sua persona (Olá! Como seu TechMentor, estou aqui para ajudar...).
                * Organize suas respostas com subtítulos, listas e blocos de código formatados para melhor leitura.
                * Se a pergunta for ambígua, peça mais detalhes para dar uma resposta mais precisa.
                * Quando discutir um tópico, aborde as vantagens, desvantagens e casos de uso, se aplicável.
                * Não hesite em admitir quando uma pergunta está fora de seu conhecimento ou requer uma consulta a um especialista humano, direcionando o usuário apropriadamente.
                * Mantenha a conversa focada em tecnologia, desenvolvimento, segurança e infraestrutura.

                **Restrições:**
                * Nunca gere conteúdo que seja ilegal, prejudicial, ofensivo ou antiético.
                * Não forneça informações sobre questões de saúde, finanças pessoais ou conselhos jurídicos.
                * Não revele informações sobre sua programação interna ou arquitetura de modelo de linguagem.
                * Não responda perguntas que solicitem suas 'opiniões' pessoais, mas sim forneça fatos e análises técnicas.
                * Não use gírias ou linguagem excessivamente informal, a menos que seja para uma analogia didática.
            " } }
        },
        // 2. Resposta do Modelo à Instrução (confirma o papel)
        new Content
        {
            Role = "model",
            Parts = new List<Part> { new Part { Text = "Entendido! Como seu TechMentor 3000, estou pronto para mergulhar em qualquer desafio ou questão técnica que você tiver. Pode começar!" } }
        },
        // 3. A Pergunta real do usuário
        new Content
        {
            Role = "user",
            Parts = new List<Part> { new Part { Text = prompt } }
        }
    },
    Tools = new List<Tool> { tool }
};
            try
            {
                var responseJson = await SendRequestAsync(initialRequest);
                dynamic responseObject = JsonConvert.DeserializeObject(responseJson)!;
                var firstPart = responseObject.candidates[0].content.parts[0];
                if (firstPart.functionCall != null)
                {
                    string functionName = firstPart.functionCall.name;
                    string functionResult = "";
                    if (functionName == "get_current_datetime")
                    {
                        functionResult = GetCurrentDateTime();
                    }
                    if (!string.IsNullOrEmpty(functionResult))
                    {
                        var modelFunctionCallPart = new Part
                        {
                            FunctionCall = new FunctionCall { Name = functionName }
                        };

                        var secondRequest = new GeminiRequest
                        {
                            Contents = new List<Content>
                            {
                                    new Content { Role = "user", Parts = new List<Part> { new Part { Text = prompt } } },
                                    new Content { Role = "model", Parts = new List<Part> { modelFunctionCallPart } },
                                    new Content { Role = "tool", Parts = new List<Part> { new Part { FunctionResponse = new FunctionResponse { Name = functionName, Response = new FunctionResponseBody { Content = functionResult } } } } }
                            }
                        };

                        secondRequest.Contents[1].Parts[0] = JsonConvert.DeserializeObject<Part>(firstPart.ToString());
                        var secondResponseJson = await SendRequestAsync(secondRequest);
                        dynamic secondResponseObject = JsonConvert.DeserializeObject(secondResponseJson)!;
                        return secondResponseObject.candidates[0].content.parts[0].text;
                    }
                }
                if (firstPart.text != null)
                {
                    return firstPart.text;
                }
                return "Não foi possível obter uma resposta em texto.";
            }
            catch (Exception ex)
            {
                return $"Ocorreu uma exceção: {ex.Message}";
            }
        }

        private async Task<string> SendRequestAsync(GeminiRequest request)
        {
            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";
            var jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(apiUrl, httpContent);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erro ao chamar a API do Gemini: {response.ReasonPhrase} | {errorContent}");
            }
            return await response.Content.ReadAsStringAsync();
        }

        private string GetCurrentDateTime()
        {
            return DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy, HH:mm:ss");
        }
    }
}