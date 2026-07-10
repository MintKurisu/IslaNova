using Asp.Versioning;
using IslaNova.Core.Application.Dtos.Chatbot;
using IslaNova.Core.Application.Features.Chatbot.Queries.AskChatbot;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace IslaNova.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [SwaggerTag("AI Chatbot for anonymous property inquiries")]
    public class ChatbotController : BaseApiController
    {
        [HttpPost("ask")]
        [AllowAnonymous]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChatbotResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Ask the chatbot",
            Description = "Sends a natural language question to the AI assistant. " +
                          "Returns a response based on available properties using RAG (Retrieval-Augmented Generation). " +
                          "This endpoint is public — no authentication required.")]
        public async Task<IActionResult> Ask([FromBody] ChatbotQuestionDto question)
        {
            if (string.IsNullOrWhiteSpace(question.Question))
                return BadRequest(new { detail = "Question cannot be empty." });

            var result = await Mediator.Send(new AskChatbotQuery
            {
                Question = question.Question
            });

            return Ok(result);
        }
    }
}
