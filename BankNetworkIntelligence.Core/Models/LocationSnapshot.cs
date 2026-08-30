namespace BankNetworkIntelligence.Core.Models;

public class LocationSnapshot
{
    public int Id { get; set; }

    public int LocationId { get; set; }

    public int ImportId { get; set; }

    public int? MunicipalityId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Fax { get; set; } = string.Empty;

    public bool HasBranch { get; set; }

    public bool HasAtm { get; set; }

    public bool HasAps { get; set; }

    public BankLocation Location { get; set; } = null!;

    public HebicImport Import { get; set; } = null!;

    public Municipality? Municipality { get; set; }
}