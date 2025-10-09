using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ChatBot.Services;

namespace ChatBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly GeminiService _geminiService;

        // O controller recebe o GeminiService através de injeção de dependência
        public ChatController(GeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        // Definimos um método POST no endereço /api/chat/aks
        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest("A pergunta não pode estar vazia.");
            }

            // Usamos o nosso serviço para oter a resposta do Gemini
            var response = await _geminiService.GenerateContentAsync(request.Question);

            // Retornamos a resposta em um formato JSON simples
            return Ok(new { Answer = response });
        }
    }

    // Uma classe simples para representar o corpo da requisição que virá do front-end
    public class AskRequest
    {
        public string Question { get; set; }
    }
}