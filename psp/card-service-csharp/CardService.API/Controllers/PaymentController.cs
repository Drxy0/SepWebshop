using Microsoft.AspNetCore.Mvc;
using CardService.Application.Feature.Payment.Commands.BankUpdatePayment;
using CardService.Application.Feature.Payment.Commands.InitPayment;

namespace CardService.API.Controllers
{
    [Route("ca/Payment")]
    [ApiController]
    public class PaymentController : ApiControllerBase
    {
        [HttpPost("init")]
        public async Task<IActionResult> InitPayment([FromBody] InitPayementCommandRequest request, CancellationToken cancellationToken)
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
