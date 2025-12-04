using Nuons.Http.Abstractions;

namespace Nuons.EndToEnd.Http;

[Route("http-handler")]
public class HttpHandler
{
	[Get("get")]
	public string Get() => "get";
}
