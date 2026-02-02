using Bank.Contracts;
using Bank.Models;

namespace Bank.Helpers;

internal static class PaymentServiceHelper
{
    internal static bool IsValidCardPayment(PayByCardRequest request, PaymentRequest paymentRequest, DebitCard card)
    {
        if (string.IsNullOrWhiteSpace(request.CVV) || request.CVV.Length != 3 || !request.CVV.All(char.IsDigit))
            return false;

        if (!LuhnFormulaChecker.IsValidLuhn(request.CardNumber))
            return false;

        if (DebitCardHelper.IsCardExpired(card.ExpirationDate) ||
            card.CVV != request.CVV ||
            card.Account?.Balance < paymentRequest.Amount)
            return false;

        return true;
    }

    internal static PaymentRequest CreatePaymentRequest(PaymentInitRequest request, Guid pspId)
    {
        return new PaymentRequest
        {
            PaymentRequestId = Guid.NewGuid(),
            MerchantId = request.MerchantId,
            PspId = pspId,
            PspPaymentId = request.PspPaymentId,
            Amount = request.Amount,
            Currency = request.Currency,
            Stan = request.Stan,
            PspTimestamp = request.PspTimestamp,
            Status = PaymentRequestStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };
    }

    internal static void TransferFunds(Account source, Account destination, double amount)
    {
        source.Balance -= amount;
        destination.Balance += amount;
    }

    internal static GetPaymentRequestResponse ValidatePaymentRequestStatus(GetPaymentRequestResponse paymentRequest)
    {
        if (paymentRequest.Status != PaymentRequestStatus.Pending)
        {
            return paymentRequest with
            {
                ErrorMessage = $"Payment request is not valid. Current status: {paymentRequest.Status}"
            };
        }

        if (paymentRequest.ExpiresAt < DateTime.UtcNow)
        {
            return paymentRequest with
            {
                ErrorMessage = $"Payment request expired at {paymentRequest.ExpiresAt:g}"
            };
        }

        return paymentRequest;
    }

    internal static string BuildPaymentUrl(Guid paymentRequestId, bool isQrPayment, string frontendUrl)
    {
        var paymentType = isQrPayment ? "qr" : "card";
        return $"{frontendUrl}/pay/{paymentType}/{paymentRequestId}";
    }
}
