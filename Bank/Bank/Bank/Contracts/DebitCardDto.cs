namespace Bank.Contracts;

public record DebitCardDto(
    int Id,
    string CardNumber,
    string CardHolderName,
    DateOnly ExpirationDate,
    string CVV
);
