using Bank.Contracts;

namespace Bank.Clients;

public interface IPSPClient
{
    Task<string> NotifyPaymentStatusAsync(PspPaymentStatusDto dto);
}
