using System;
using System.Threading.Tasks;
using Nuons.Http.Abstractions;

namespace Nuons.Http.Generators.Tests;

[Route]
internal partial class EmptyHandler
{
	[Get]
	public void GetEmpty() { }
}

[Route("/complex/")]
internal partial class ComplexHandler
{
	[Get("/sample/{id}/")]
	public void Get(Guid id, string name, GenericClass<string> generic) { }
}

internal class GenericClass<T>;

[Route("/domain-object")]
internal partial class DomainObjectHandler
{
	[Get("{id}")]
	public void Get(string id) { }

	[Post]
	public void Create(DomainObject domainObject) { }

	[Delete("{id}")]
	public void Delete(Guid id) { }

	[Put("{id}")]
	public void Update(int id, DomainObject domainObject) { }

	[Patch("{id}")]
	public void Patch(Guid id, DomainObject domainObject) { }
}

internal class DomainObject;

[Route("binding-test")]
internal partial class BindingTestHandler
{
	[Get("{id}")]
	public string GetWithBindings(
		[FromRoute] Guid id,
		[FromHeader("Authorization")] string auth,
		[FromQuery] int page,
		[FromQuery("page_size")] int pageSize)
	{
		return "";
	}

	[Post]
	public void CreateWithBody([FromBody] CreateRequest request, [FromServices] ILogger logger)
	{
		return;
	}

	[Post("form")]
	public void CreateFromForm([FromForm] string name, [AsParameters] FormParams formParams)
	{
		return;
	}
}

internal class CreateRequest;

internal class FormParams;

internal interface ILogger;

[Authorize("AdminPolicy")]
[Route("secured")]
internal partial class SecuredHandler
{
	[Get("{id}")]
	public string Get(Guid id) => "";

	[AllowAnonymous]
	[Post]
	public void Create(string body) { }
}

[Route("mixed-auth")]
internal partial class MixedAuthHandler
{
	[Authorize]
	[Get("{id}")]
	public string Get(Guid id) => "";

	[Authorize("EditorPolicy")]
	[Put("{id}")]
	public void Update(Guid id, string body) { }

	[Get]
	public string List() => "";
}

[Tags("api", "v1")]
[Route("tagged")]
internal partial class TaggedHandler
{
	[EndpointName("GetTagged")]
	[EndpointDescription("Gets a tagged resource")]
	[Get("{id}")]
	public string Get(Guid id) => "";

	[Tags("write")]
	[Post]
	public void Create(string body) { }
}

[Route("named")]
internal partial class NamedEndpointsHandler
{
	[EndpointName("ListItems")]
	[Get]
	public string List() => "";

	[EndpointName("GetItem")]
	[EndpointDescription("Get a single item by ID")]
	[Get("{id}")]
	public string Get(Guid id) => "";
}

[EnableRateLimiting("fixed")]
[RequireCors("AllowAll")]
[Route("rate-limited")]
internal partial class RateLimitedHandler
{
	[Get("{id}")]
	public string Get(Guid id) => "";

	[DisableAntiforgery]
	[Post]
	public void Create(string body) { }
}

[Route("cors-only")]
internal partial class CorsHandler
{
	[RequireCors("SpecificOrigin")]
	[Get]
	public string List() => "";

	[EnableRateLimiting("sliding")]
	[DisableAntiforgery]
	[Post]
	public void Create(string body) { }
}

[Route("async-ops")]
internal partial class AsyncHandler
{
	[Get("{id}")]
	public Task<string> GetAsync(Guid id) => Task.FromResult("");

	[Get]
	public Task<string> ListAsync() => Task.FromResult("");

	[Post]
	public Task CreateAsync(string body) => Task.CompletedTask;

	[Put("{id}")]
	public ValueTask<string> UpdateAsync(Guid id, string body) => new ValueTask<string>("");

	[Delete("{id}")]
	public ValueTask DeleteAsync(Guid id) => default;
}

[Route("mixed-async")]
internal partial class MixedAsyncHandler
{
	[Get("{id}")]
	public Task<string> GetAsync(Guid id) => Task.FromResult("");

	[Get]
	public string List() => "";

	[Post]
	public Task CreateAsync(string body) => Task.CompletedTask;

	[Delete("{id}")]
	public void Delete(Guid id) { }
}
