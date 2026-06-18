using AutoMapper;
using IslaNova.Core.Application.Dtos.Offer;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Interfaces.UserInteraction;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.Offer.Queries.GetOffersByPropertyId
{
    public class GetOffersByPropertyIdQuery : IRequest<IList<OfferDto>>
    {
        [SwaggerParameter(Description = "Property ID")]
        public int PropertyId { get; set; }
    }

    public class GetOffersByPropertyIdQueryHandler : IRequestHandler<GetOffersByPropertyIdQuery, IList<OfferDto>>
    {
        private readonly IOfferRepository _offerRepository;
        private readonly IAuthServiceForWebApi _authService;
        private readonly IMapper _mapper;

        public GetOffersByPropertyIdQueryHandler(
            IOfferRepository offerRepository,
            IAuthServiceForWebApi authService,
            IMapper mapper)
        {
            _offerRepository = offerRepository;
            _authService = authService;
            _mapper = mapper;
        }

        public async Task<IList<OfferDto>> Handle(GetOffersByPropertyIdQuery query, CancellationToken cancellationToken)
        {
            var offers = await _offerRepository
                .GetAllQuery()
                .Where(o => o.PropertyId == query.PropertyId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);

            var offerDtos = new List<OfferDto>();

            foreach (var offer in offers)
            {
                var client = await _authService.GetUserById(offer.ClientId);
                var dto = _mapper.Map<OfferDto>(offer);

                if (client != null)
                    dto.ClientName = $"{client.Name} {client.LastName}";

                offerDtos.Add(dto);
            }

            return offerDtos;
        }
    }
}
