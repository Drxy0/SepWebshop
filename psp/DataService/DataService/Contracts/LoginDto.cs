namespace DataService.Contracts;

public sealed record LoginDto(bool isSuccess, string accessToken);
