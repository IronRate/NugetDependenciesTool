using System.Text.RegularExpressions;

namespace NugetDependenciesTool;

internal sealed class InputReader
{
    private static Regex _regex = new Regex("^(--|\\/\\/)", RegexOptions.Compiled);

    private readonly string _filename;

    public InputReader(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new ArgumentException($"\"{nameof(filename)}\" не может быть пустым или содержать только пробел.", nameof(filename));
        }

        _filename = filename;
    }

    public async Task<IReadOnlyCollection<string>> ReadAsync(CancellationToken cancellation = default)
    {
        var result = new List<string>(10);

        using var file = File.OpenText(_filename);

        do
        {
            var line = await file.ReadLineAsync(cancellation);
            if (line != null && !_regex.IsMatch(line))
            {
                result.Add(line);
            }

        } while (!file.EndOfStream);
        return result;
    }
}
