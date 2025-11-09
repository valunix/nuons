using Microsoft.AspNetCore.Mvc;
using Nuons.DependencyInjection;

namespace Nuons.EndToEnd.Api;

[Controller]
[Route(Routes.Controller)]
[InjectedConstructor]
public partial class ControllerExample : ControllerBase
{
	[Injected]
	private readonly IComplexService service;

	[HttpGet]
	public IActionResult Get() => Ok(service.GetValue());
}
