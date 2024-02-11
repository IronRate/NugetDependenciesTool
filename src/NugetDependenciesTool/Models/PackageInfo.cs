using NuGet.Packaging.Core;

namespace NugetDependenciesTool.Models;

internal class PackageInfo
{
    public PackageInfo(PackageDependency dependency, bool root)
    {
        Id = dependency.Id;
        Version = dependency.VersionRange.MinVersion?.Version.ToString()
            ?? dependency.VersionRange.MaxVersion?.Version.ToString();
        Root = root;
    }

    public PackageInfo(string id, string version, bool root)
    {
        Id = id;
        Version = version;
        Root = root;
    }

    /// <summary>
    /// Наименование пакета.
    /// </summary>
    public string Id { get; } = null!;

    /// <summary>
    /// Версия.
    /// </summary>
    public string Version { get; } = null!;

    /// <summary>
    /// Признак коренвого элемента.
    /// </summary>
    public bool Root { get; }
}
