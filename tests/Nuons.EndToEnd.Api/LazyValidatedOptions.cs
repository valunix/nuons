using System.ComponentModel.DataAnnotations;
using Nuons.DependencyInjection.Abstractions;

namespace Nuons.EndToEnd.Api;

// Validate (DataAnnotations) without ValidateOnStart: the host starts even with invalid
// configuration, and validation only fires the first time the options are resolved.
[Options("LazyValidatedOptions", Validate = true)]
public class LazyValidatedOptions
{
	[Range(1, 10)]
	public int Count { get; set; }
}
