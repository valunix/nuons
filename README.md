# Nuons
A lightweight open-source library for .NET that cuts down boilerplate and speeds up application development.

## Dependency Injection samples

Nuons Dependency Injection provides a set of attributes and source generators to simplify dependency injection in .NET apps. Here are some samples to get you productive in minutes.

### 1) Register services using attributes
Use one of the lifetime attributes on your implementation type, specifying the interface (or base type) to register for.
```csharp
using Nuons.DependencyInjection;

public interface IGreetingService
{
    string Greet(string name);
}

[Transient(typeof(IGreetingService))]
public partial class GreetingService : IGreetingService
{
    public string Greet(string name) => $"Hello, {name}!";
}

// Other lifetimes:
// [Scoped(typeof(IGreetingService))]
// [Singleton(typeof(IGreetingService))]
```

### 2) Inject services into your classes
Nuons supports clean field injection by simply marking fields with `[Injected]`. Generator will create the constructor and wire it up for you. Make sure to mark the class as `partial`.
```csharp
using Nuons.DependencyInjection;

[Singleton(typeof(GreetingConsumer))]
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

[Singleton(typeof(Dashboard))]
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
Use the generated combined registration to register all services and bind options in one call. The generator creates a class named `CombinedRegistration{YourAssemblyName}` under the `Nuons.DependencyInjection.Extensions` namespace.

```csharp
using Nuons.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register all services and options discovered by Nuons in this assembly
CombinedRegistrationMyApp.RegisterServices(builder.Services, builder.Configuration);
// Replace MyApp with your assembly name

var app = builder.Build();
app.MapGet("/hello/{name}", (string name, IGreetingService greeter) => greeter.Greet(name));
app.Run();
```

### 5) Multiple assemblies
If your services live across multiple projects you can use `RootRegistrationAttribute` to combine them and call registrations from single point.

Root registration class (in your startup/host project):
```csharp
using Nuons.DependencyInjection;

namespace MyApp.Startup;

[RootRegistration(typeof(MyApp.Startup.AssemblyMarker), typeof(MySubDomainA.AssemblyMarker), typeof(MySubDomainB.AssemblyMarker))]
public partial class MyRootRegistration;
```

Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

MyRootRegistration.RegisterServices(builder.Services, builder.Configuration);

var app = builder.Build();
app.MapGet("/hello/{name}", (string name, IGreetingService greeter) => greeter.Greet(name));
app.Run();
```
