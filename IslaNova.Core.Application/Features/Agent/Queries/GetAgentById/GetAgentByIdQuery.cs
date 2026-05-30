using MediatR;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Dtos.User;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.Agent.Queries.GetById
{
    /// <summary>
    /// Query used to retrieve a single agent by its unique identifier.
    /// </summary>
    public class GetAgentByIdQuery : IRequest<AgentUserDto?>
    {
        [SwaggerParameter(Description = "Unique identifier of the agent.")]
        public string? Id { get; set; }
    }

    public class GetAgentByIdQueryHandler : IRequestHandler<GetAgentByIdQuery, AgentUserDto?>
    {
        private readonly IAuthServiceForWebApi _authServiceForWebApi;
        private readonly IPropertyRepository _propertyRepository;
        public GetAgentByIdQueryHandler(IAuthServiceForWebApi authServiceForWebApi, IPropertyRepository propertyRepository)
        {
            _authServiceForWebApi = authServiceForWebApi;
            _propertyRepository = propertyRepository;
        }

        public async Task<AgentUserDto?> Handle(GetAgentByIdQuery query, CancellationToken cancellationToken)
        {
            var entity = await _authServiceForWebApi.GetUserById(query.Id ?? "");

            if (entity != null && entity.Role == Roles.Agent.ToString())
            {
                var propertyCount = await _propertyRepository.GetAllQuery().Where(p => p.AgentId == entity.Id).CountAsync();

                AgentUserDto dto = new()
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    LastName = entity.LastName,
                    Email = entity.Email,
                    PhoneNumber = entity.PhoneNumber,
                    PropertyCount = propertyCount
                };

                return dto;
            }

            return null;
        }
    }
}
