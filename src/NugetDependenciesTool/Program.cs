// See https://aka.ms/new-console-template for more information
// https://russianblogs.com/article/63162304640/
// https://learn.microsoft.com/ru-ru/nuget/api/registration-base-url-resource

using System.Collections.Concurrent;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NugetDependenciesTool;
using NugetDependenciesTool.Models;

var s = string.Join(" ", Environment.GetCommandLineArgs().Skip(1));
var parser = new System.CommandLine.Parser.CommandLineParser();
var argums = parser.Bind<CommandLine>(s);

Console.WriteLine("Nuget dependencies getter");

var logger = new LoggerStub();
SourceCacheContext cache = new SourceCacheContext();
var factory = new Repository.RepositoryFactory();
var repository = factory.GetCoreV3(argums.nuget);
var resource = await repository.GetResourceAsync<FindPackageByIdResource>();
var resource2 = await repository.GetResourceAsync<PackageMetadataResource>();

var fullDependenciesList = new List<PackageInfo>();

var inputReader = new InputReader(argums.input);
var rootPackagesList = (await inputReader.ReadAsync()) ?? Array.Empty<(string, Version?)>();
var nodesVisited = new ConcurrentDictionary<(string, string), bool>();//<-посещенные узлы.
string[] frameworks = [/*".NETFramework", */".NETCoreApp", ".NETStandard"];


foreach (var rootPackage in rootPackagesList)
{
    var packages = await GetRootDependenciesAsync(rootPackage, CancellationToken.None);
    if (packages != null && packages.Any())
        fullDependenciesList.AddRange(packages);
}

var b = fullDependenciesList
    .GroupBy(x => new { x.Id, x.Version })
    .Select(x => x.First());

fullDependenciesList = new List<PackageInfo>(b.Count());
fullDependenciesList.AddRange(b);

var writer = new OutputWriter(argums.output);
await writer.WriteAsync(fullDependenciesList);

Console.WriteLine("Work is complete!");

async Task<IEnumerable<PackageInfo>?> GetRootDependenciesAsync((string packageId, Version? version) package, CancellationToken cancellation)
{
    // Берем только те версии, которые месяц как опубликованы.
    var now = DateTimeOffset.Now;
    var month = now.AddDays(-31);

    var result = new List<PackageInfo>(1);
    var metadata = await resource2.GetMetadataAsync(package.packageId, false, false, cache, logger, cancellation);

    var enumerable = package.version == null
        ? metadata
            .Cast<PackageSearchMetadataRegistration>()
            .Where(x => x.Published < month)
            .OrderByDescending(x => x.Version)
         : metadata
            .Cast<PackageSearchMetadataRegistration>()
            .Where(x => x.Version.Major == package.version.Major)
            .Where(x => x.Version.Minor == package.version.Minor)
            .Where(x => x.Version.Patch == package.version.Build)
            .OrderByDescending(x => x.Version);


    if (enumerable.Any())
    {
        var lastVersion = enumerable
            .First(x => !x.Version.IsPrerelease);

        result.Add(new PackageInfo(package.packageId, lastVersion.Version.ToString(), true));

        var packages = await GetPackageDependenciesAsync(
            package.packageId,
            lastVersion.Version,
            cancellation
        );
        if (packages != null && packages.Any())
            result.AddRange(packages);
    }

    return result;
}
async Task<IEnumerable<PackageInfo>?> GetPackageDependenciesAsync(
    string packageId,
    NuGetVersion version,
    CancellationToken cancellation
)
{
    var listOfPackages = default(List<PackageInfo>);
    var key = (packageId, version?.Version.ToString());

    if (nodesVisited.ContainsKey(key))
        return listOfPackages;

    var dependencies = await resource.GetDependencyInfoAsync(packageId, version, cache, logger, cancellation);
    var packages = dependencies.DependencyGroups
        .Where(x => frameworks.Contains(x.TargetFramework.Framework))
        .SelectMany(x => x.Packages).ToList();

    listOfPackages = new List<PackageInfo>(packages.Count());
    listOfPackages.AddRange(packages.Select(x => new PackageInfo(x, false)));
    foreach (var package in packages)
    {
        cancellation.ThrowIfCancellationRequested();

        var dep1 = await GetPackageDependenciesAsync(
            package.Id,
            package.VersionRange.MinVersion ?? package.VersionRange.MaxVersion,
            cancellation
        );
        if (dep1 != null)
            listOfPackages.AddRange(dep1);
    }
    nodesVisited.TryAdd(key, true);

    return listOfPackages;
}


