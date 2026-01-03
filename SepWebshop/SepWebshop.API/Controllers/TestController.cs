using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SepWebshop.API.Services;
using SepWebshop.Application.Abstractions.IdentityService;

namespace SepWebshop.API.Controllers
{
    [Route("test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        public TestController(IIdentityService identityService)
        {
            _identityService = identityService;
        }
        [HttpGet()]
        public IActionResult Test()
        {
            return Ok("Test successful!");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-jwt")]
        public IActionResult Testtwo()
        {
            var userIdFromToken = _identityService.UserIdentity;
            var email = _identityService.Email;

            return Ok(new
            {
                Message = "Test successful!",
                UserId = userIdFromToken,
                Email = email
            });
        }
    }
}
