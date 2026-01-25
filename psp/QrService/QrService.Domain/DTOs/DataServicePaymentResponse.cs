using QrService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrService.Domain.DTOs
{
    public class DataServicePaymentResponse
    {
        public Guid Id { get; set; }
        public required string MerchantId { get; set; }
        public required string MerchantPassword { get; set; }
        public required double Amount { get; set; }
        public Currency Currency { get; set; }
        public required string MerchantOrderId { get; set; }
        public DateTime MerchantTimestamp { get; set; }
    }
}
