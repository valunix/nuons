using System.ComponentModel.DataAnnotations;
using Nuons.DependencyInjection.Abstractions;

namespace Nuons.EndToEnd.Api;

[Options("ValidatedOptions", Validate = true, ValidateOnStart = true)]
public class ValidatedOptions
{
	[Required]
	public string Name { get; set; } = string.Empty;

	[Range(1, 100)]
	public int Count { get; set; }
}
