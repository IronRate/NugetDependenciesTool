namespace NugetDependenciesTool.Models;

internal class CommandLine
{
    /// <summary>
    /// Путь к файлу, содержащему список пакетов
    /// </summary>
    public string input { get; init; }
    
    /// <summary>
    /// Путь к файлу, в который будет выгружен результат сбора информации.
    /// </summary>
    public string output { get; init; }

    /// <summary>
    /// Адрес сервера.
    /// </summary>
    public string nuget { get; init; } = "https://api.nuget.org/v3/index.json";
}
