using IslaNova.Infrastructure.Identity.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Infrastructure.Identity.Features.Auth.Commands.RevokeRefreshToken
{
    public class RevokeRefreshTokenCommand : IRequest<bool>
    {
        public required string UserId { get; set; }
    }

    public class RevokeRefreshTokenCommandHandler : IRequestHandler<RevokeRefreshTokenCommand, bool>
    {
        private readonly IdentityContext _context;

        public RevokeRefreshTokenCommandHandler(
                  IdentityContext context
          )
        {
            _context = context;
        }

        public async Task<bool> Handle(RevokeRefreshTokenCommand command, CancellationToken cancellationToken)
        {
            await _context.RefreshTokens
                .Where(rt => rt.UserId == command.UserId)
                .ExecuteDeleteAsync();

            return true;
        }
    }
}
