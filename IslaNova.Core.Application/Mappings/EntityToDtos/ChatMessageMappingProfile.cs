using AutoMapper;
using IslaNova.Core.Application.Dtos.ChatMessage;
using IslaNova.Core.Domain.Entities.UserInteraction;

namespace IslaNova.Core.Application.Mappings.EntityToDtos
{
    public class ChatMessageMappingProfile : Profile
    {
        public ChatMessageMappingProfile()
        {
            CreateMap<ChatMessage, ChatMessageDto>()
                .ForMember(dest => dest.ClientName, opt => opt.Ignore())
                .ForMember(dest => dest.AgentName, opt => opt.Ignore());

            CreateMap<CreateChatMessageDto, ChatMessage>()
                .ForMember(dest => dest.ChatMessageId, opt => opt.Ignore())
                .ForMember(dest => dest.SentAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsRead, opt => opt.Ignore())
                .ForMember(dest => dest.Property, opt => opt.Ignore());
        }
    }
}
