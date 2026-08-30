using BankNetworkIntelligence.Core.Data;
using BankNetworkIntelligence.Core.Models;
using BankNetworkIntelligence.Importer.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace BankNetworkIntelligence.Importer.Services;

public class HebicImportService
{
    private readonly BankNetworkDbContext _db;

    public HebicImportService(BankNetworkDbContext db)
    {
        _db = db;
    }

    public async Task<HebicImport> ImportAsync(
        string period,
        string sourceFile,
        IReadOnlyCollection<HebicRecord> records,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(period);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFile);

        if (records.Count == 0)
        {
            throw new ArgumentException(
                "The HEBIC file contains no records.",
                nameof(records));
        }

        ValidateRecords(records);

        // ============================================================
        // Normalize records once
        // ============================================================

        var normalizedRecords = records
            .Select(NormalizeRecord)
            .ToList();


        // ============================================================
        // Detect duplicate locations inside this CSV
        // ============================================================

        var duplicateLocations = normalizedRecords
            .GroupBy(x => x.LocationKey, StringComparer.Ordinal)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        if (duplicateLocations.Count > 0)
        {
            throw new InvalidOperationException(
                "The HEBIC file contains duplicate locations:" +
                Environment.NewLine +
                string.Join(
                    Environment.NewLine,
                    duplicateLocations));
        }


        // ============================================================
        // Start transaction
        // ============================================================

        await using var transaction =
            await _db.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            // ========================================================
            // 1. Check whether period already exists
            // ========================================================

            var existingImport = await _db.HebicImports
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Period == period,
                    cancellationToken);

            if (existingImport != null)
            {
                throw new InvalidOperationException(
                    $"HEBIC period '{period}' has already been imported " +
                    $"(Import ID: {existingImport.Id}).");
            }


            // ========================================================
            // 2. Create import
            // ========================================================

            var import = new HebicImport
            {
                Period = period,
                ImportedAt = DateTime.UtcNow,
                SourceFile = sourceFile,
                RecordCount = normalizedRecords.Count
            };

            _db.HebicImports.Add(import);


            // ========================================================
            // 3. Load existing banks
            // ========================================================

            var bankNames = normalizedRecords
                .Select(x => x.Bank)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existingBanks = await _db.Banks
                .Where(x => bankNames.Contains(x.Name))
                .ToListAsync(cancellationToken);

            var banksByName =
                new Dictionary<string, Bank>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (var bank in existingBanks)
            {
                banksByName[bank.Name] = bank;
            }


            // ========================================================
            // 4. Create missing banks
            // ========================================================

            foreach (var bankName in bankNames)
            {
                if (banksByName.ContainsKey(bankName))
                {
                    continue;
                }

                var bank = new Bank
                {
                    Name = bankName
                };

                _db.Banks.Add(bank);

                banksByName.Add(
                    bankName,
                    bank);
            }


            // ========================================================
            // 5. Load municipalities
            // ========================================================

            var existingMunicipalities =
                await _db.Municipalities
                    .ToListAsync(cancellationToken);

            var municipalitiesByKey =
                new Dictionary<MunicipalityKey, Municipality>(
                    MunicipalityKeyComparer.Instance);

            foreach (var municipality in existingMunicipalities)
            {
                var key = new MunicipalityKey(
                    municipality.Name,
                    municipality.Prefecture);

                municipalitiesByKey[key] = municipality;
            }


            // ========================================================
            // 6. Create missing municipalities
            //
            // Empty municipality/prefecture is allowed.
            // ========================================================

            foreach (var record in normalizedRecords)
            {
                if (string.IsNullOrWhiteSpace(record.Municipality) ||
                    string.IsNullOrWhiteSpace(record.Prefecture))
                {
                    continue;
                }

                var key = new MunicipalityKey(
                    record.Municipality,
                    record.Prefecture);

                if (municipalitiesByKey.ContainsKey(key))
                {
                    continue;
                }

                var municipality = new Municipality
                {
                    Name = record.Municipality,
                    Prefecture = record.Prefecture
                };

                _db.Municipalities.Add(municipality);

                municipalitiesByKey.Add(
                    key,
                    municipality);
            }


            // ========================================================
            // 7. Load existing locations
            // ========================================================

            var existingLocations =
                await _db.BankLocations
                    .ToListAsync(cancellationToken);

            var locationsByKey =
                new Dictionary<string, BankLocation>(
                    StringComparer.Ordinal);

            foreach (var location in existingLocations)
            {
                if (!string.IsNullOrWhiteSpace(
                        location.LocationKey))
                {
                    locationsByKey[location.LocationKey] =
                        location;
                }
            }


            // ========================================================
            // 8. Create missing locations
            // ========================================================

            foreach (var record in normalizedRecords)
            {
                if (locationsByKey.ContainsKey(
                        record.LocationKey))
                {
                    continue;
                }

                var bank = banksByName[record.Bank];

                var location = new BankLocation
                {
                    Bank = bank,
                    HebicCode = record.HebicCode,
                    LocationKey = record.LocationKey
                };

                _db.BankLocations.Add(location);

                locationsByKey.Add(
                    record.LocationKey,
                    location);
            }


            // ========================================================
            // 9. Create snapshots
            // ========================================================

            foreach (var record in normalizedRecords)
            {
                var location =
                    locationsByKey[record.LocationKey];

                Municipality? municipality = null;

                if (!string.IsNullOrWhiteSpace(
                        record.Municipality) &&
                    !string.IsNullOrWhiteSpace(
                        record.Prefecture))
                {
                    var municipalityKey =
                        new MunicipalityKey(
                            record.Municipality,
                            record.Prefecture);

                    municipality =
                        municipalitiesByKey[municipalityKey];
                }

                var snapshot = new LocationSnapshot
                {
                    Location = location,
                    Import = import,
                    Municipality = municipality,

                    Name = record.Name,
                    Address = record.Address,
                    PostalCode = record.PostalCode,
                    Phone = record.Phone,
                    Fax = record.Fax,

                    HasBranch = record.HasBranch,
                    HasAtm = record.HasAtm,
                    HasAps = record.HasAps
                };

                _db.LocationSnapshots.Add(snapshot);
            }


            // ========================================================
            // 10. Save everything
            // ========================================================

            await _db.SaveChangesAsync(
                cancellationToken);


            // ========================================================
            // 11. Commit transaction
            // ========================================================

            await transaction.CommitAsync(
                cancellationToken);

            return import;
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }


    // =================================================================
    // Record normalization
    // =================================================================

    private static NormalizedRecord NormalizeRecord(
     HebicRecord record)
    {
        var hebicCode = NormalizeNullable(
            record.HebicCode);

        var bank = NormalizeRequired(
            record.Bank);

        var name = NormalizeRequired(
            record.Name);

        var address = NormalizeRequired(
            record.Address);

        var postalCode = NormalizeRequired(
            record.PostalCode);

        var municipality = NormalizeOptional(
            record.Municipality);

        var prefecture = NormalizeOptional(
            record.Prefecture);

        var phone = NormalizeRequired(
            record.Phone);

        var fax = NormalizeRequired(
            record.Fax);


        // ------------------------------------------------------------
        // Generate our internal location identity.
        //
        // Preferred:
        //   Bank + HEBIC + Address + PostalCode
        //
        // HEBIC missing:
        //   Bank + Address + PostalCode
        //
        // Address/postal missing:
        //   Bank + HEBIC
        //
        // The last case is only a fallback because HEBIC alone has
        // already been proven capable of appearing on multiple rows
        // when address information is available.
        // ------------------------------------------------------------

        string locationKey;

        var hasAddress =
            !string.IsNullOrWhiteSpace(address);

        var hasPostalCode =
            !string.IsNullOrWhiteSpace(postalCode);

        if (!string.IsNullOrWhiteSpace(hebicCode) &&
            hasAddress &&
            hasPostalCode)
        {
            locationKey =
                $"BANK:{NormalizeKey(bank)}" +
                $"|HEBIC:{NormalizeKey(hebicCode)}" +
                $"|ADDRESS:{NormalizeKey(address)}" +
                $"|POSTAL:{NormalizeKey(postalCode)}";
        }
        else if (hasAddress &&
                 hasPostalCode)
        {
            locationKey =
                $"BANK:{NormalizeKey(bank)}" +
                $"|ADDRESS:{NormalizeKey(address)}" +
                $"|POSTAL:{NormalizeKey(postalCode)}";
        }
        else if (!string.IsNullOrWhiteSpace(hebicCode))
        {
            locationKey =
                $"BANK:{NormalizeKey(bank)}" +
                $"|HEBIC:{NormalizeKey(hebicCode)}";
        }
        else
        {
            throw new InvalidOperationException(
                "Cannot generate a reliable location identity.");
        }


        return new NormalizedRecord
        {
            HebicCode = hebicCode,
            Bank = bank,
            Name = name,
            Address = address,
            PostalCode = postalCode,
            Phone = phone,
            Fax = fax,

            Municipality = municipality,
            Prefecture = prefecture,

            HasBranch = record.HasBranch,
            HasAtm = record.HasAtm,
            HasAps = record.HasAps,

            LocationKey = locationKey
        };
    }

    // =================================================================
    // Validation
    // =================================================================

    private static void ValidateRecords(
    IReadOnlyCollection<HebicRecord> records)
    {
        var recordNumber = 0;

        foreach (var record in records)
        {
            recordNumber++;

            if (string.IsNullOrWhiteSpace(record.Bank))
            {
                throw new InvalidOperationException(
                    $"Record {recordNumber}: bank name is empty.");
            }

            var hasHebic =
                !string.IsNullOrWhiteSpace(record.HebicCode);

            var hasAddress =
                !string.IsNullOrWhiteSpace(record.Address);

            var hasPostalCode =
                !string.IsNullOrWhiteSpace(record.PostalCode);

            // ------------------------------------------------------------
            // We need at least one reliable identity strategy:
            //
            // 1. HEBIC + Address + PostalCode
            // 2. Address + PostalCode
            // 3. HEBIC
            //
            // HEBIC alone is only a fallback because HEBIC can occur
            // multiple times when full location information exists.
            // ------------------------------------------------------------

            if (!hasHebic &&
                !(hasAddress && hasPostalCode))
            {
                throw new InvalidOperationException(
                    $"Record {recordNumber}: " +
                    "insufficient information to identify the location. " +
                    "The record has neither a HEBIC code nor both " +
                    "address and postal code.");
            }
        }
    }

    // =================================================================
    // String normalization
    // =================================================================

    private static string NormalizeRequired(
        string? value)
    {
        return value?.Trim() ?? string.Empty;
    }


    private static string? NormalizeNullable(
        string? value)
    {
        var normalized = value?.Trim();

        return string.IsNullOrWhiteSpace(normalized)
            ? null
            : normalized;
    }


    private static string NormalizeOptional(
        string? value)
    {
        return value?.Trim() ?? string.Empty;
    }


    private static string NormalizeKey(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value
            .Trim()
            .Normalize(NormalizationForm.FormC)
            .ToUpperInvariant();

        // Collapse consecutive whitespace characters.
        var builder = new StringBuilder(
            normalized.Length);

        var previousWasWhitespace = false;

        foreach (var character in normalized)
        {
            if (char.IsWhiteSpace(character))
            {
                if (previousWasWhitespace)
                {
                    continue;
                }

                builder.Append(' ');
                previousWasWhitespace = true;
            }
            else
            {
                builder.Append(character);
                previousWasWhitespace = false;
            }
        }

        return builder.ToString();
    }


    // =================================================================
    // Internal DTO
    // =================================================================

    private sealed class NormalizedRecord
    {
        public string? HebicCode { get; init; }

        public string Bank { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string Address { get; init; } = string.Empty;

        public string PostalCode { get; init; } = string.Empty;

        public string Phone { get; init; } = string.Empty;

        public string Fax { get; init; } = string.Empty;

        public string Municipality { get; init; } = string.Empty;

        public string Prefecture { get; init; } = string.Empty;

        public bool HasBranch { get; init; }

        public bool HasAtm { get; init; }

        public bool HasAps { get; init; }

        public string LocationKey { get; init; } = string.Empty;
    }


    // =================================================================
    // Natural keys
    // =================================================================

    private readonly record struct MunicipalityKey(
        string Name,
        string Prefecture);


    // =================================================================
    // Municipality comparer
    // =================================================================

    private sealed class MunicipalityKeyComparer
        : IEqualityComparer<MunicipalityKey>
    {
        public static readonly MunicipalityKeyComparer Instance = new();

        public bool Equals(
            MunicipalityKey x,
            MunicipalityKey y)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(
                       x.Name,
                       y.Name)
                   &&
                   StringComparer.OrdinalIgnoreCase.Equals(
                       x.Prefecture,
                       y.Prefecture);
        }

        public int GetHashCode(
            MunicipalityKey obj)
        {
            var hash = new HashCode();

            hash.Add(
                obj.Name,
                StringComparer.OrdinalIgnoreCase);

            hash.Add(
                obj.Prefecture,
                StringComparer.OrdinalIgnoreCase);

            return hash.ToHashCode();
        }
    }
}
