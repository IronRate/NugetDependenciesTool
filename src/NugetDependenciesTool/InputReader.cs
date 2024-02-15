using System.Text.RegularExpressions;

namespace NugetDependenciesTool;

internal sealed class InputReader
{
    private static Regex _commentPatern = new Regex("^(--|\\/\\/)", RegexOptions.Compiled);
    private static Regex _packagePattern = new Regex(
        @"^(?<PackageId>.*) --version (?<Major>0|(?:[1-9]\d*))(?:\.(?<Minor>0|(?:[1-9]\d*))(?:\.(?<Patch>0|(?:[1-9]\d*)))?(?:\-(?<PreRelease>[0-9A-Z\.-]+))?(?:\+(?<Meta>[0-9A-Z\.-]+))?)?$",
        RegexOptions.Compiled);


    private readonly string _filename;

    public InputReader(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new ArgumentException($"\"{nameof(filename)}\" не может быть пустым или содержать только пробел.", nameof(filename));
        }

        _filename = filename;
    }

    public async Task<IReadOnlyCollection<(string packageId, Version? version)>> ReadAsync(CancellationToken cancellation = default)
    {
        var result = new List<(string, Version?)>(10);

        using var file = File.OpenText(_filename);

        do
        {
            var line = await file.ReadLineAsync(cancellation);
            if (line == null) continue;
            var workLine = line.Trim();
            if (_commentPatern.IsMatch(workLine))
            {
                continue;
            }
            else
            {
                var match = _packagePattern.Match(workLine);
                if (match.Success)
                {
                    var packageId = match.Groups["PackageId"].Value;
                    string[] versionArr = {
                        match.Groups["Major"].Value,
                        match.Groups["Minor"].Value,
                        match.Groups["Patch"].Value,
                        match.Groups["PreRelease"].Value
                    };


                    var versionStr = string.Join('.', versionArr.Select(x => !string.IsNullOrWhiteSpace(x) ? x : null).Where(x => x != null));
                    var version = new Version(versionStr);
                    result.Add(new(packageId, version));
                }
                else
                {
                    result.Add((workLine, null));
                }
            }

        } while (!file.EndOfStream);
        return result;
    }
}
