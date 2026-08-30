namespace BankNetworkIntelligence.Core.Models;

public class BankLocation
{
    public int Id { get; set; }

    public int BankId { get; set; }

    public string? HebicCode { get; set; }

    public string LocationKey { get; set; } = string.Empty;

    public Bank Bank { get; set; } = null!;

    public ICollection<LocationSnapshot> Snapshots { get; set; }
        = new List<LocationSnapshot>();
}