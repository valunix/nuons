# Nuons
A lightweight open-source library for .NET that cuts down boilerplate and speeds up application development.

## Installation

Add `Nuons.Startup` package to your main project where your `Program.cs` is. If you use multiple projects and want to leverage nuons add `Nuons` package to them and mark them with `[assembly: AssemblyHasNuons]`.

**Important note:** nuon packages need to be marked with `PrivateAssets="all"` to work correctly.

## Dependency Injection samples

Nuons Dependency Injection provides a set of attributes and source generators to simplify dependency injection in .NET apps. Here are some samples to get you productive in minutes.

### 1) Register services using attributes
Use one of the lifetime attributes on your implementation type. The generator supports:
- Parameterless attributes (if a class directly implements exactly one interface, it will be registered for that interface, otherwise it will fallback to class itself).
- Generic attributes to explicitly specify the service interface to register.

```csharp
using Nuons.DependencyInjection;

public interface IGreetingService
{
    string Greet(string name);
}

// Parameterless: registers GreetingService as IGreetingService (inferred)
[Transient]
public partial class GreetingService : IGreetingService
{
    public string Greet(string name) => $"Hello, {name}!";
}

// Generic: explicitly registers as IGreetingService
[Transient<IGreetingService>]
public partial class GreetingServiceGeneric : IGreetingService
{
    public string Greet(string name) => $"Hello, {name}!";
}

// Other lifetimes work the same way:
// [Scoped]
// [Scoped<IGreetingService>]
// [Singleton]
// [Singleton<IGreetingService>]
```

Selecting the interface when a class implements multiple interfaces
```csharp
using Nuons.DependencyInjection;

internal interface ITarget;
internal interface ISomeOtherInterface;

// When a class implements multiple interfaces, use the generic attribute
// to specify which interface should be registered in DI.
[Singleton<ITarget>]
public partial class MultipleInterfacesDirect : ITarget, ISomeOtherInterface;
```

### 2) Inject services into your classes
Nuons supports clean field injection by simply marking class with `[InjectConstructor]` and fields with `[Injected]`. Generator will create the constructor and wire it up for you. Make sure to mark the class as `partial`.
```csharp
using Nuons.DependencyInjection;

[Singleton]
[InjectConstructor]
public partial class GreetingConsumer
{
    [Injected] private readonly IGreetingService greetings;

    public string SayHello(string name) => greetings.Greet(name);
}
```

### 3) Bind configuration options
Annotate your options class with `[Options("SectionKey")]` and consume it via `[Injected]` or `[InjectedOptions]`. Make sure that `Microsoft.Extensions.Options` namespace is available if registering in a separate project.
```csharp
using Nuons.DependencyInjection;

[Options("MyApp")]
public class MyAppOptions
{
    public string Title { get; set; } = "";
}

[Singleton]
[InjectConstructor]
public partial class Dashboard
{
    [Injected] private readonly IOptions<MyAppOptions> optionsWrapped;

    [InjectedOptions] private readonly MyAppOptions optionsUnwrapped;

    public string Header() => optionsUnwrapped.Title;
}
```
appsettings.json
```json
{
  "MyApp": {
    "Title": "Nuons Demo"
  }
}
```
Nuons will generate the code to bind configuration and make `MyAppOptions` available for injection.

### 4) Wire up Program.cs
Use the generated `AddNuonDependancyInjectionServices` method under the `Nuons.DependencyInjection.Extensions` namespace to register all services and configurations from single place.

```csharp
using Nuons.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddNuonDependancyInjectionServices(builder.Configuration);

var app = builder.Build();
app.MapGet("/hello/{name}", (string name, IGreetingService greeter) => greeter.Greet(name));
app.Run();
```
