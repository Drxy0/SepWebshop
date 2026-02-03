using Microsoft.AspNetCore.Mvc;
using QrService.Application.Feature.Payment.Commands.BankUpdatePayment;
using QrService.Application.Feature.Payment.Commands.InitPayment;

namespace QrService.API.Controllers
{
    [Route("q/Payment")]
    [ApiController]
    public class PaymentController : ApiControllerBase
    {
        [HttpPost("init")]
        public async Task<IActionResult> InitPayment([FromBody]InitPayementCommandRequest request, CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPut("bank/update/{paymentId}")]
        public async Task<IActionResult> UpdatePayment([FromBody] BankUpdatePaymentCommandRequest request, CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }
    }
}
