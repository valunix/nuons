using Nuons.Http.Abstractions;

namespace Nuons.EndToEnd.Api;

[Route("nuon-http-handler")]
public class NuonHttpHandler
{
	[Get("get")]
	public string Get() => "get";
}
