using AutoMapper;
using IslaNova.Core.Application.Common.Models;
using IslaNova.Core.Application.Dtos.Feature;
using IslaNova.Core.Domain.Interfaces.Feature;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Core.Application.Features.Improvement.Queries.GetAllImprovement
{
    /// <summary>
    /// Query used to retrieve all improvements available in the system.
    /// </summary>
    public class GetAllImprovementQuery : IRequest<PaginatedResult<ImprovementDto>>
    {
        public string? Search { get; set; }
        public string? Order { get; set; } = "desc";
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }

    public class GetAllImprovementQueryHandler : IRequestHandler<GetAllImprovementQuery, PaginatedResult<ImprovementDto>>
    {
        private readonly IImprovementRepository _improvementRepository;
        private readonly IMapper _mapper;

        public GetAllImprovementQueryHandler(IImprovementRepository improvementRepository, IMapper mapper)
        {
            _improvementRepository = improvementRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<ImprovementDto>> Handle(GetAllImprovementQuery query, CancellationToken cancellationToken)
        {
            if (query.Page < 1) query.Page = 1;
            if (query.Limit < 1) query.Limit = 10;
            if (query.Limit > 100) query.Limit = 100;

            var q = _improvementRepository.GetAllQuery();

            // Search by name 
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.ToLower();
                q = q.Where(i => i.Name.ToLower().Contains(s));
            }

            var total = await q.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(total / (double)query.Limit);

            // Sort by name 
            q = query.Order?.ToLower() == "asc"
                ? q.OrderBy(i => i.Name)
                : q.OrderByDescending(i => i.Name);

            var items = await q
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<ImprovementDto>
            {
                Data = _mapper.Map<List<ImprovementDto>>(items),
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
