// See https://aka.ms/new-console-template for more information
// https://russianblogs.com/article/63162304640/
// https://learn.microsoft.com/ru-ru/nuget/api/registration-base-url-resource

using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
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
FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();

var fullDependenciesList = new List<PackageInfo>();



var inputReader = new InputReader(argums.input);
var rootPackagesList = (await inputReader.ReadAsync()) ?? Array.Empty<string>();


//string[] rootPackagesList = {
//"Microsoft.EntityFrameworkCore.Relational",
//"Microsoft.EntityFrameworkCore.Sqlite",
//"Microsoft.EntityFrameworkCore.Abstractions",
//"Microsoft.EntityFrameworkCore.Analyzers",
//"Microsoft.EntityFrameworkCore.Design",
//"Microsoft.EntityFrameworkCore.InMemory",
//"Microsoft.EntityFrameworkCore.Sqlite.Core",
//"Microsoft.EntityFrameworkCore.SqlServer",
//"Microsoft.EntityFrameworkCore.Tools",
//"Microsoft.Extensions.Configuration.Binder",
//"Microsoft.Extensions.Configuration.Json",
//"Microsoft.Extensions.Features",
//"Microsoft.Extensions.Hosting.WindowsServices",
//"Microsoft.Extensions.Logging.Abstraction",
//"Microsoft.NET.Test.Sdk",
//"Microsoft.Extensions.Features",
//"Microsoft.AspNetCore.Connections.Abstractions",
//"System.Security.Cryptography.Pkcs",
//// Other
//"Confluent.Kafka",
//"FluentValidation",
//"FluentValidation.DependencyInjectionExtensions",
//"MSTest.TestAdapter",
//"MSTest.TestFramework",
//"MediatR",
//"NLog",
//"NLog.Extensions.Logging",
//"NLog.Web.AspNetCore",
//"System.Text.Json",
//"CsvHelper",
//"xUnit",
//"NSubstitute",
//"Quartz.AspNetCore"
//};

foreach (var rootPackage in rootPackagesList)
{
    var packages = await GetAllDependencies(rootPackage);
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

async Task<IEnumerable<PackageInfo>?> GetAllDependencies(string packageId)
{
    var result = new List<PackageInfo>(1);
    var versions = await resource.GetAllVersionsAsync(packageId, cache, logger, CancellationToken.None);
    if (versions.Any())
    {
        var lastVersion = versions.Last(x => !x.IsPrerelease);
        result.Add(new PackageInfo(packageId, lastVersion.Version.ToString(), true));
        var packages = await GetPackageDependencies(packageId, lastVersion.Version);
        if (packages != null && packages.Any())
            result.AddRange(packages);
    }
    return result;
}

async Task<IEnumerable<PackageInfo>?> GetPackageDependencies(string packageId, Version version)
{
    var listOfPackages = default(List<PackageInfo>);

    var lastVersion = new NuGet.Versioning.NuGetVersion(version);
    var dependencies = await resource.GetDependencyInfoAsync(packageId, lastVersion, cache, logger, CancellationToken.None);
    var packages = dependencies.DependencyGroups.Select(x => x.Packages).FirstOrDefault();
    if (packages != null)
    {
        listOfPackages = new List<PackageInfo>(packages.Count());
        listOfPackages.AddRange(packages.Select(x => new PackageInfo(x, false)));
        foreach (var package in packages)
        {
            var dep1 = await GetPackageDependencies(package.Id, package.VersionRange.MinVersion?.Version ?? package.VersionRange.MaxVersion?.Version);
            if (dep1 != null)
                listOfPackages.AddRange(dep1);
        }
    }

    return listOfPackages;
}


