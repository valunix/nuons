; Shipped analyzer releases
; https://github.com/dotnet/roslyn/blob/main/src/RoslynAnalyzers/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## 0.1.1

### New Rules

Rule ID | Category | Severity | Notes
----- | ----- | ----- | -----
NU001 | Nuons.DependencyInjection | Error | Class with a service attribute must be partial
NU002 | Nuons.DependencyInjection | Warning | Class with fields marked as [Injected] should be marked as service
NU003 | Nuons.DependencyInjection | Error | Multiple service registration attributes
