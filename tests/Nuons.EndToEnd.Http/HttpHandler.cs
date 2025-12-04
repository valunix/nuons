using Nuons.Http.Abstractions;

namespace Nuons.EndToEnd.TransientFeature.Infrastructure;

[Route("http-handler")]
public class HttpHandler
{
	[Get("get")]
	public string Get() => "get";
}
