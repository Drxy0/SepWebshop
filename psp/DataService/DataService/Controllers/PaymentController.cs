using DataService.Contracts;
using DataService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataService.Controllers
{
    [ApiController]
    [Route("d/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPaymentService _paymentService;

        public PaymentController(IUserService userService, IPaymentService paymentService)
        {
            _userService = userService;
            _paymentService = paymentService;
        }

        [Authorize]
        [HttpGet("payment-methods")]
        public async Task<IActionResult> GetMyMethods()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            var methods = await _userService.GetUserPaymentMethods(Guid.Parse(userIdClaim));
            return Ok(methods);
        }

        [Authorize]
        [HttpPost("payment-methods")]
        public async Task<IActionResult> UpdateMyMethods([FromBody] UpdatePaymentMethodsRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            if (request.PaymentMethodIds == null || !request.PaymentMethodIds.Any())
            {
                return BadRequest(new { Message = "At least one payment method must be selected." });
            }

            var result = await _userService.UpdateUserPaymentMethods(Guid.Parse(userIdClaim), request.PaymentMethodIds);

            if (!result)
            {
                return BadRequest(new { Message = "Failed to update methods. Ensure at least one valid method is selected." });
            }

            return result ? Ok(new { Message = "Methods updated" }) : BadRequest();
        }
        
        [HttpGet("methods/{merchantId}")]
        public async Task<IActionResult> GetMethodsByMerchant(string merchantId)
        {
            var methods = await _userService.GetActiveMethodsByMerchantId(merchantId);

            if (methods == null || !methods.Any())
                return NotFound(new { Message = "Merchant not found or no payment methods enabled." });

            return Ok(methods);
        }

        [HttpPost("init")]
        public async Task<IActionResult> InitializePayment([FromBody] InitializePaymentRequest request)
        {
            var initResult = await _paymentService.InitializePaymentAsync(request);
            if (!initResult.IsSuccess)
            {
                return BadRequest(new { Message = "Failed to initialize payment." });
            }
            return Ok(new { PaymentId = initResult.PaymentId });
        }
    }
}
