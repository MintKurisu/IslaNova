using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IslaNova.WebApi.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public abstract class BaseApiController : Controller
    {
        private readonly IMediator? _mediator;

        protected IMediator Mediator => _mediator ?? HttpContext!.RequestServices.GetService<IMediator>()!;
    }
}
