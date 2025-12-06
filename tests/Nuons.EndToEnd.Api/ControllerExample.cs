using Microsoft.AspNetCore.Mvc;
using Nuons.CodeInjection.Abstractions;

namespace Nuons.EndToEnd.Api;

[Controller]
[Route(Routes.Controller)]
[InjectConstructor]
public partial class ControllerExample : ControllerBase
{
	[Injected]
	private readonly IComplexService service;

	[HttpGet]
	public IActionResult Get() => Ok(service.GetValue());
}
