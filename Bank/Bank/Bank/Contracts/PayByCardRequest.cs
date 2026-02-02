namespace Bank.Contracts;

public sealed record PayByCardRequest(
    string CardNumber,
    string CardHolderName,
    int ExpiryMonth,
    int ExpiryYear,
    string CardHolder,
    string CVV
);
