using AutoMapper;
using IslaNova.Core.Application.Dtos.Offer;
using IslaNova.Core.Domain.Interfaces.UserInteraction;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.Offer.Queries.GetOffersByClientId
{
    public class GetOffersByClientIdQuery : IRequest<IList<OfferDto>>
    {
        [SwaggerParameter(Description = "Client ID")]
        public string? ClientId { get; set; }
        [SwaggerParameter(Description = "Property ID")]
        public int PropertyId { get; set; }
    }

    public class GetOffersByClientIdQueryHandler : IRequestHandler<GetOffersByClientIdQuery, IList<OfferDto>>
    {
        private readonly IOfferRepository _offerRepository;
        private readonly IMapper _mapper;

        public GetOffersByClientIdQueryHandler(
            IOfferRepository offerRepository,
            IMapper mapper)
        {
            _offerRepository = offerRepository;
            _mapper = mapper;
        }

        public async Task<IList<OfferDto>> Handle(GetOffersByClientIdQuery query, CancellationToken cancellationToken)
        {
            var offers = await _offerRepository
                .GetAllQuery()
                .Where(o => o.ClientId == query.ClientId && o.PropertyId == query.PropertyId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<OfferDto>>(offers);
        }
    }
}
