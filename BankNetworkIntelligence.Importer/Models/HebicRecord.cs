namespace BankNetworkIntelligence.Importer.Models;

public class HebicRecord
{
    public string HebicCode { get; set; } = string.Empty;

    public string Bank { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Fax { get; set; } = string.Empty;

    public string Municipality { get; set; } = string.Empty;

    public string Prefecture { get; set; } = string.Empty;

    public bool HasBranch { get; set; }

    public bool HasAtm { get; set; }

    public bool HasAps { get; set; }
}