namespace BankNetworkIntelligence.Core.Models;

public class HebicImport
{
    public int Id { get; set; }

    public string Period { get; set; } = string.Empty;

    public DateTime ImportedAt { get; set; }

    public string SourceFile { get; set; } = string.Empty;

    public int RecordCount { get; set; }

    public ICollection<LocationSnapshot> LocationSnapshots { get; set; }
        = new List<LocationSnapshot>();
}