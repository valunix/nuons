using Microsoft.AspNetCore.Mvc;
using Nuons.DependencyInjection.Abstractions;

namespace Nuons.EndToEnd.Api;

[Controller]
[Route("pero")]
[InjectedConstructor]
public partial class ControllerExample : ControllerBase
{
	[Injected]
	private readonly IComplexService service;

	[HttpGet("/pero")]
	public IActionResult Get() => Ok(service.GetValue());
}
