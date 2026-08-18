using IslaNova.Core.Application.Common.Models;
using IslaNova.Core.Application.Dtos.User;
using IslaNova.Core.Application.Interfaces.Auth;
using MediatR;

namespace IslaNova.Infrastructure.Identity.Features.Auth.Queries.GetAllUsers
{
    public class GetAllUsersQuery : IRequest<PaginatedResult<UserDto>>
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
        public string? Search { get; set; }
        public string? Order { get; set; } = "asc";
    }

    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PaginatedResult<UserDto>>
    {
        private readonly IAuthServiceForWebApi _authServiceForWebApi;

        public GetAllUsersQueryHandler(IAuthServiceForWebApi authServiceForWebApi)
        {
            _authServiceForWebApi = authServiceForWebApi;
        }

        public async Task<PaginatedResult<UserDto>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
        {
            if (query.Page < 1) query.Page = 1;
            if (query.Limit < 1) query.Limit = 10;
            if (query.Limit > 100) query.Limit = 100;

            var entityList = await _authServiceForWebApi.GetAllUser();

            IEnumerable<UserDto> filtered = entityList;

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.ToLower();
                filtered = filtered.Where(u =>
                    u.Name.ToLower().Contains(s) ||
                    u.LastName.ToLower().Contains(s) ||
                    u.Email.ToLower().Contains(s));
            }

            // Sort
            filtered = query.Order?.ToLower() == "asc"
                ? filtered.OrderBy(u => u.CreatedAt)
                : filtered.OrderByDescending(u => u.CreatedAt);

            var total = filtered.Count();
            var totalPages = (int)Math.Ceiling(total / (double)query.Limit);

            // in memory pagination
            var paged = filtered
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .ToList();


            return new PaginatedResult<UserDto>
            {
                Data = paged,
                Meta = new PageMetadata
                {
                    Page = query.Page,
                    Limit = query.Limit,
                    Total = total,
                    TotalPage = totalPages
                }
            };
        }
    }
}
