namespace BankNetworkIntelligence.Core.Models;

public class Municipality
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Prefecture { get; set; } = string.Empty;

    public ICollection<LocationSnapshot> LocationSnapshots { get; set; }
        = new List<LocationSnapshot>();
}