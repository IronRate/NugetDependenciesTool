namespace NugetDependenciesToolTests;

public sealed class SemVerTests
{
    [Theory]
    [InlineData(1, 1, 1, 0)]
    [InlineData(1, 1, 0, 0)]
    [InlineData(1, 0, 0, 0)]
    [InlineData(0, 0, 0, 0)]
    public void Test1(int major, int minor, int build, int revision)
    {
        // Arrange & Act
        var version = new Version(major, minor, build, revision);

        // Assert
        Assert.Equal(major, version.Major);
        Assert.Equal(minor, version.Minor);
        Assert.Equal(build, version.Build);
        Assert.Equal(revision, version.Revision);
    }

    [Theory]
    [InlineData(1, 1, 1, 0)]
    [InlineData(1, 1, 0, 0)]
    [InlineData(1, 0, 0, 0)]
    [InlineData(0, 0, 0, 0)]
    public void Test2(int major, int minor, int build, int revision)
    {
        // Arrange
        var expected = $"{major}.{minor}.{build}.{revision}";

        // Arrange & Act
        var version = new Version(major, minor, build, revision);

        // Assert
        var actual = version.ToString();
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Негативный тест.
    /// </summary>
    /// <remarks>
    /// Здесь можно наблюдать как, в зависимости от того какой конструктор используется <br />
    /// метод ToString - будет выдавать неожидаемое поведение.
    /// </remarks>
    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(1, 1, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(0, 0, 0)]
    public void Test3(int major, int minor, int build)
    {
        // Arrange
        var expected = $"{major}.{minor}.{build}.0";

        // Arrange & Act
        var version = new Version(major, minor, build);

        // Assert
        var actual = version.ToString();
        Assert.NotEqual(expected, actual);
    }

    /// <summary>
    /// Негативный тест.
    /// </summary>
    /// <remarks>
    /// Здесь можно наблюдать как, в зависимости от того какой конструктор используется <br />
    /// метод ToString - будет выдавать неожидаемое поведение.
    /// </remarks>
    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 0)]
    [InlineData(0, 0)]
    public void Test4(int major, int minor)
    {
        // Arrange
        var expected = $"{major}.{minor}.0.0";

        // Arrange & Act
        var version = new Version(major, minor);

        // Assert
        var actual = version.ToString();
        Assert.NotEqual(expected, actual);
    }
}
