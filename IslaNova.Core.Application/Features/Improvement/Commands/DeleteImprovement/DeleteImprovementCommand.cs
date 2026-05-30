using MediatR;
using IslaNova.Core.Application.Exceptions;
using IslaNova.Core.Domain.Interfaces.Feature;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace IslaNova.Core.Application.Features.Improvement.Commands.DeleteImprovement
{
    /// <summary>
    /// Command used to delete an existing  improvement (feature) that it's associated with a property.
    /// </summary>
    public class DeleteImprovementCommand : IRequest<Unit>
    {
        /// <example>3</example>
        [SwaggerParameter(Description = "The unique identifier of the improvement to delete")]
        public int ImprovementId { get; set; }
    }

    public class DeleteImprovementCommandHandler : IRequestHandler<DeleteImprovementCommand, Unit>
    {
        private readonly IImprovementRepository _improvementRepository;

        public DeleteImprovementCommandHandler(IImprovementRepository improvementRepository)
        {
            _improvementRepository = improvementRepository;
        }

        public async Task<Unit> Handle(DeleteImprovementCommand command, CancellationToken cancellationToken)
        {
            var entity = await _improvementRepository.GetByIdAsync(command.ImprovementId);

            if (entity == null)
                throw new ApiException("Error deleting improvement", (int)HttpStatusCode.InternalServerError);

            await _improvementRepository.DeleteAsync(command.ImprovementId);
            return Unit.Value;
        }
    }
}
