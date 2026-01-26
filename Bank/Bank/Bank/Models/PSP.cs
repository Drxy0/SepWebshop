namespace Bank.Models;

public class PSP
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string HMACKeySecretName { get; set; } // TODO: Name of the key in Azure Key Vault
}
