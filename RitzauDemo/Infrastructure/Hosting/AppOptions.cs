using System.ComponentModel.DataAnnotations;

namespace RitzauDemo.Infrastructure.Hosting;

internal sealed class AppOptions
{
    public const string SectionName = "App";

    [Required]
    [MinLength(1)]
    public string Name { get; init; } = "RitzauDemo";
}
