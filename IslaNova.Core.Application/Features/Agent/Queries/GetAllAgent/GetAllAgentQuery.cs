using MediatR;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Dtos.User;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;

namespace IslaNova.Core.Application.Features.Agent.Queries.GetAllAgent
{
    /// <summary>
    /// Query used to retrieve all agents available in the system.
    /// </summary>
    public class GetAllAgentQuery : IRequest<IList<AgentUserDto>> { }

    public class GetAllAgentQueryHandler : IRequestHandler<GetAllAgentQuery, IList<AgentUserDto>>
    {
        private readonly IAuthServiceForWebApi _authServiceForWebApi;
        private readonly IPropertyRepository _propertyRepository;
        public GetAllAgentQueryHandler(IAuthServiceForWebApi authServiceForWebApi, IPropertyRepository propertyRepository)
        {
            _authServiceForWebApi = authServiceForWebApi;
            _propertyRepository = propertyRepository;
        }

        public async Task<IList<AgentUserDto>> Handle(GetAllAgentQuery query, CancellationToken cancellationToken)
        {
            var entityList = await _authServiceForWebApi.GetAllUserByRole(Roles.Agent);
            List<AgentUserDto> dtoList = [];

            foreach (var entity in entityList)
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

                dtoList.Add(dto);
            }

            return dtoList;

        }
    }
}
