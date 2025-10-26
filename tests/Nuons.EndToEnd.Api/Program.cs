using Nuons.EndToEnd.Api;
using Nuons.EndToEnd.ScopedFeature.Domain;
using Nuons.EndToEnd.ServiceFeature.Domain;
using Nuons.EndToEnd.SingletonFeature.Domain;
using Nuons.EndToEnd.TransientFeature.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

RootRegistration.RegisterServices(builder.Services, builder.Configuration);

var app = builder.Build();

app.MapGet(Routes.Singleton, (ISingletonService singletonService) => singletonService.GetValue());
app.MapGet(Routes.Transient, (ITransientService transientService) => transientService.GetValue());
app.MapGet(Routes.Scoped, (IScopedService scopedService) => scopedService.GetValue());
app.MapGet(Routes.Service, (IServiceAttributeService serviceAttributeService) => serviceAttributeService.GetValue());
app.MapGet(Routes.Complex, (IComplexService complexService) => complexService.GetValue());

app.MapControllers();

app.Run();

public partial class Program;
