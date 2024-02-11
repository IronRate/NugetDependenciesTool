using NugetDependenciesTool.Models;

namespace NugetDependenciesTool;

internal sealed class OutputWriter
{
    private readonly string _filename;

    public OutputWriter(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new ArgumentException($"\"{nameof(filename)}\" не может быть пустым или содержать только пробел.", nameof(filename));
        }

        _filename = filename;
    }

    public async Task WriteAsync(IReadOnlyCollection<PackageInfo> packages, CancellationToken cancellationToken = default)
    {
        using var file = File.CreateText(_filename);

        using var writer = new CsvHelper.CsvWriter(file, new System.Globalization.CultureInfo("en-US"), false);
        await writer.WriteRecordsAsync(packages, cancellationToken);
        await writer.FlushAsync();
    }
}
