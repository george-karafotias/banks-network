using System.Text;
using BankNetworkIntelligence.Core.Data;
using BankNetworkIntelligence.Importer.Parsers;
using BankNetworkIntelligence.Importer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// ------------------------------------------------------------
// Console encoding
// ------------------------------------------------------------

Console.OutputEncoding = Encoding.UTF8;


// ------------------------------------------------------------
// Command-line arguments
// ------------------------------------------------------------

if (args.Length != 2)
{
    Console.WriteLine("Usage:");
    Console.WriteLine(
        "  BankNetworkIntelligence.Importer <csv-path> <period>");
    Console.WriteLine();
    Console.WriteLine("Example:");
    Console.WriteLine(
        @"  BankNetworkIntelligence.Importer ""C:\Data\hebic.csv"" ""2026-Q3""");

    return;
}

var filePath = args[0];
var period = args[1];

if (string.IsNullOrWhiteSpace(filePath))
{
    Console.WriteLine("CSV file path cannot be empty.");
    return;
}

if (string.IsNullOrWhiteSpace(period))
{
    Console.WriteLine("Period cannot be empty.");
    return;
}

if (!File.Exists(filePath))
{
    Console.WriteLine($"HEBIC file not found: {filePath}");
    return;
}

Console.WriteLine($"CSV file: {filePath}");
Console.WriteLine($"Period:   {period}");
Console.WriteLine();


// ------------------------------------------------------------
// Configuration
// ------------------------------------------------------------

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString =
    configuration.GetConnectionString("BankNetwork");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine(
        "Database connection string was not found.");

    return;
}


// ------------------------------------------------------------
// PostgreSQL / EF Core
// ------------------------------------------------------------

var options = new DbContextOptionsBuilder<BankNetworkDbContext>()
    .UseNpgsql(connectionString)
    .Options;

await using var db = new BankNetworkDbContext(options);

Console.WriteLine("Testing PostgreSQL connection...");

try
{
    var canConnect =
        await db.Database.CanConnectAsync();

    if (!canConnect)
    {
        Console.WriteLine("Connection failed.");
        return;
    }

    Console.WriteLine("Connection successful!");
}
catch (Exception ex)
{
    Console.WriteLine("Connection failed.");
    Console.WriteLine(ex.Message);

    return;
}

Console.WriteLine();


// ------------------------------------------------------------
// Parse HEBIC file
// ------------------------------------------------------------

Console.WriteLine("Parsing HEBIC file...");

var parser = new HebicParser();

var records = parser.Parse(filePath);

Console.WriteLine(
    $"Parsed records: {records.Count}");

Console.WriteLine();

if (records.Count == 0)
{
    Console.WriteLine(
        "No records found. Import aborted.");

    return;
}


// ------------------------------------------------------------
// Display first 10 records
// ------------------------------------------------------------

Console.WriteLine("First 10 records:");
Console.WriteLine();

foreach (var record in records.Take(10))
{
    Console.WriteLine(
        $"HEBIC:        {record.HebicCode}");

    Console.WriteLine(
        $"Bank:         {record.Bank}");

    Console.WriteLine(
        $"Name:         {record.Name}");

    Console.WriteLine(
        $"Address:      {record.Address}");

    Console.WriteLine(
        $"Postal Code:  {record.PostalCode}");

    Console.WriteLine(
        $"Municipality: {record.Municipality}");

    Console.WriteLine(
        $"Prefecture:   {record.Prefecture}");

    Console.WriteLine(
        $"Branch:       {record.HasBranch}");

    Console.WriteLine(
        $"ATM:          {record.HasAtm}");

    Console.WriteLine(
        $"APS:          {record.HasAps}");

    Console.WriteLine(
        new string('-', 60));
}


// ------------------------------------------------------------
// Import
// ------------------------------------------------------------

Console.WriteLine();

Console.WriteLine("Starting HEBIC import...");

var importService =
    new HebicImportService(db);

try
{
    var import = await importService.ImportAsync(
        period,
        Path.GetFileName(filePath),
        records);

    Console.WriteLine();

    Console.WriteLine(
        "========================================");

    Console.WriteLine(
        "IMPORT COMPLETED SUCCESSFULLY");

    Console.WriteLine(
        "========================================");

    Console.WriteLine(
        $"Import ID:    {import.Id}");

    Console.WriteLine(
        $"Period:       {import.Period}");

    Console.WriteLine(
        $"Source file:  {import.SourceFile}");

    Console.WriteLine(
        $"Records:      {import.RecordCount}");

    Console.WriteLine(
        $"Imported at:  {import.ImportedAt:u}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine();

    Console.WriteLine("IMPORT FAILED");

    Console.WriteLine(ex.Message);
}
catch (Exception ex)
{
    Console.WriteLine();

    Console.WriteLine(
        "UNEXPECTED IMPORT ERROR");

    Console.WriteLine(ex.Message);

    if (ex.InnerException != null)
    {
        Console.WriteLine();

        Console.WriteLine("Inner exception:");

        Console.WriteLine(
            ex.InnerException.Message);
    }
}
