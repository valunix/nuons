using Nuons.EndToEnd.Api;
using Nuons.EndToEnd.ScopedFeature.Domain;
using Nuons.EndToEnd.SingletonFeature.Domain;
using Nuons.EndToEnd.TransientFeature.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

RootRegistration.RegisterServices(builder.Services, builder.Configuration);

builder.Services.AddNuonServices();

var app = builder.Build();

app.MapGet(Routes.Singleton, (ISingletonService singletonService) => singletonService.GetValue());
app.MapGet(Routes.SingletonGeneric, (ISingletonGenericService singletonService) => singletonService.GetValue());
app.MapGet(Routes.Transient, (ITransientService transientService) => transientService.GetValue());
app.MapGet(Routes.TransientGeneric, (ITransientGenericService transientService) => transientService.GetValue());
app.MapGet(Routes.Scoped, (IScopedService scopedService) => scopedService.GetValue());
app.MapGet(Routes.ScopedGeneric, (IScopedGenericService scopedService) => scopedService.GetValue());
app.MapGet(Routes.Complex, (IComplexService complexService) => complexService.GetValue());

app.MapControllers();

app.MapNuonEndpoints();

app.Run();

public partial class Program;
