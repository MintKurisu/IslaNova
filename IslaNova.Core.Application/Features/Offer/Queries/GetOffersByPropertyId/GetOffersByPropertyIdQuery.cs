using AutoMapper;
using IslaNova.Core.Application.Common.Models;
using IslaNova.Core.Application.Dtos.Offer;
using IslaNova.Core.Domain.Interfaces.UserInteraction;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Core.Application.Features.Offer.Queries.GetOffersByPropertyId
{
    public class GetOffersByPropertyIdQuery : IRequest<PaginatedResult<OfferDto>>
    {
        public int PropertyId { get; set; }
        public string? Order { get; set; } = "desc";
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }

    public class GetOffersByPropertyIdQueryHandler : IRequestHandler<GetOffersByPropertyIdQuery, PaginatedResult<OfferDto>>
    {
        private readonly IOfferRepository _offerRepository;
        private readonly IMapper _mapper;

        public GetOffersByPropertyIdQueryHandler(IOfferRepository offerRepository, IMapper mapper)
        {
            _offerRepository = offerRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<OfferDto>> Handle(GetOffersByPropertyIdQuery query, CancellationToken cancellationToken)
        {
            if (query.Page < 1) query.Page = 1;
            if (query.Limit < 1) query.Limit = 10;
            if (query.Limit > 100) query.Limit = 100;

            var q = _offerRepository
                .GetAllQuery()
                .Where(o => o.PropertyId == query.PropertyId);

            var total = await q.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(total / (double)query.Limit);

            // Sort by creation date
            q = query.Order?.ToLower() == "asc"
                ? q.OrderBy(o => o.CreatedAt)
                : q.OrderByDescending(o => o.CreatedAt);

            var items = await q
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<OfferDto>
            {
                Data = _mapper.Map<List<OfferDto>>(items),
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
