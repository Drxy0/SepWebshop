using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrService.Application.Feature.Payment.Commands.InitPayment
{
    public class InitPayementCommandRequest : IRequest<InitPayementCommandResponse>
    {
        public string MerachanOrderId { get; set; } = string.Empty;
    }

    public class InitPayementCommandResponse
    {
        public string BankUrl { get; set; } = string.Empty;
    }
}
