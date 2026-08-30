using System.Text;
using BankNetworkIntelligence.Importer.Models;

namespace BankNetworkIntelligence.Importer.Parsers;

public class HebicParser
{
    private static readonly Encoding GreekEncoding;

    static HebicParser()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        GreekEncoding = Encoding.GetEncoding(
            28597,
            EncoderFallback.ExceptionFallback,
            DecoderFallback.ExceptionFallback);
    }

    public List<HebicRecord> Parse(string filePath)
    {
        var records = new List<HebicRecord>();

        var lines = File.ReadAllLines(filePath, GreekEncoding);

        if (lines.Length < 3)
            return records;

        // First line is rubbish.
        // Second line is the actual header.
        var header = lines[1];

        var expectedColumns = header.Split(';').Length;

        for (int i = 2; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            var columns = lines[i].Split(';');

            // HEBIC files contain a trailing ';' on data rows.
            // We only need the 12 columns defined by the header.
            if (columns.Length < expectedColumns)
                continue;

            var record = new HebicRecord
            {
                HebicCode = Clean(columns[0]),
                Bank = Clean(columns[1]),
                Name = Clean(columns[2]),
                Address = Clean(columns[3]),
                PostalCode = Clean(columns[4]),
                Phone = Clean(columns[5]),
                Fax = Clean(columns[6]),
                Municipality = Clean(columns[7]),
                Prefecture = Clean(columns[8]),
                HasBranch = ParseYesNo(columns[9]),
                HasAtm = ParseYesNo(columns[10]),
                HasAps = ParseYesNo(columns[11])
            };

            records.Add(record);
        }

        return records;
    }

    private static string Clean(string value)
    {
        return value
            .Trim()
            .Trim('\'');
    }

    private static bool ParseYesNo(string value)
    {
        return value.Trim().Equals(
            "Ναι",
            StringComparison.OrdinalIgnoreCase);
    }
}