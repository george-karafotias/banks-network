namespace BankNetworkIntelligence.Core.Models;

public class Bank
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<BankLocation> Locations { get; set; }
        = new List<BankLocation>();
}