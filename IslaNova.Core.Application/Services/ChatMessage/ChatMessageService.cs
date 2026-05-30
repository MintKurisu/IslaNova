using AutoMapper;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Dtos.ChatMessage;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Application.Interfaces.ChatMessage;
using IslaNova.Core.Application.Services.Base;
using IslaNova.Core.Domain.Interfaces.UserInteraction;

namespace IslaNova.Core.Application.Services.ChatMessage
{
    public class ChatMessageService : GenericService<Core.Domain.Entities.UserInteraction.ChatMessage, ChatMessageDto>, IChatMessageService
    {
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IAuthServiceForWebApi _authService;
        private readonly IMapper _mapper;

        public ChatMessageService(
            IChatMessageRepository chatMessageRepository,
            IAuthServiceForWebApi authService,
            IMapper mapper) : base(chatMessageRepository, mapper)
        {
            _chatMessageRepository = chatMessageRepository;
            _authService = authService;
            _mapper = mapper;
        }

        public async Task<List<ChatMessageDto>> GetChatMessagesByPropertyAndClientAsync(int propertyId, string clientId)
        {
            try
            {
                var messages = await _chatMessageRepository
                    .GetAllQuery()
                    .Where(cm => cm.PropertyId == propertyId && cm.ClientId == clientId)
                    .OrderBy(cm => cm.SentAt)
                    .ToListAsync();

                var messageDtos = new List<ChatMessageDto>();

                foreach (var message in messages)
                {
                    var client = await _authService.GetUserById(message.ClientId);
                    var agent = await _authService.GetUserById(message.AgentId);
                    var dto = _mapper.Map<ChatMessageDto>(message);

                    if (client != null)
                    {
                        dto.ClientName = $"{client.Name} {client.LastName}";
                    }

                    if (agent != null)
                    {
                        dto.AgentName = $"{agent.Name} {agent.LastName}";
                    }

                    messageDtos.Add(dto);
                }

                return messageDtos;
            }
            catch (Exception)
            {
                return [];
            }
        }

        public async Task<List<string>> GetClientsWithChatByPropertyIdAsync(int propertyId)
        {
            try
            {
                return await _chatMessageRepository
                    .GetAllQuery()
                    .Where(cm => cm.PropertyId == propertyId)
                    .Select(cm => cm.ClientId)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception)
            {
                return [];
            }
        }

        public async Task<ChatMessageDto?> SendMessageAsync(CreateChatMessageDto dto)
        {
            try
            {
                var chatMessage = new Core.Domain.Entities.UserInteraction.ChatMessage
                {
                    PropertyId = dto.PropertyId,
                    ClientId = dto.ClientId,
                    AgentId = dto.AgentId,
                    SenderId = dto.SenderId,
                    Message = dto.Message,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };

                var createdMessage = await _chatMessageRepository.AddAsync(chatMessage);

                if (createdMessage == null) return null;

                var client = await _authService.GetUserById(createdMessage.ClientId);
                var agent = await _authService.GetUserById(createdMessage.AgentId);
                var resultDto = _mapper.Map<ChatMessageDto>(createdMessage);

                if (client != null)
                {
                    resultDto.ClientName = $"{client.Name} {client.LastName}";
                }

                if (agent != null)
                {
                    resultDto.AgentName = $"{agent.Name} {agent.LastName}";
                }

                return resultDto;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
