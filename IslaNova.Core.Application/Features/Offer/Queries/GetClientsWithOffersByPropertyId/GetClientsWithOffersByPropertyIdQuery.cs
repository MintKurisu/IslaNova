using IslaNova.Core.Domain.Interfaces.UserInteraction;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.Offer.Queries.GetClientsWithOffersByPropertyId
{
    public class GetClientsWithOffersByPropertyIdQuery : IRequest<IList<string>>
    {
        [SwaggerParameter(Description = "Property ID")]
        public int PropertyId { get; set; }
    }

    public class GetClientsWithOffersByPropertyIdQueryHandler : IRequestHandler<GetClientsWithOffersByPropertyIdQuery, IList<string>>
    {
        private readonly IOfferRepository _offerRepository;

        public GetClientsWithOffersByPropertyIdQueryHandler(IOfferRepository offerRepository)
        {
            _offerRepository = offerRepository;
        }

        public async Task<IList<string>> Handle(GetClientsWithOffersByPropertyIdQuery query, CancellationToken cancellationToken)
        {
            return await _offerRepository
                .GetAllQuery()
                .Where(o => o.PropertyId == query.PropertyId)
                .Select(o => o.ClientId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
    }
}
