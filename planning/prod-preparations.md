# Going to production: Dependency Injection + Code Injection

Plan for finalising the **DI** and **CodeInjection** areas so they can be shipped as v1. HTTP is out of scope. Architecture, packaging and the assembly-marker mechanism are referenced where relevant but treated as already settled.

## Versioning

Breaking changes are acceptable; No back-compat shims required.

## Decisions taken

- **Options validation** opt-in via additional properties on `OptionsAttribute`: `Validate` (DataAnnotations) and `ValidateOnStart` as independent flags.
- **Keyed services** opt-in via a `Key` property on the existing lifetime attributes — no new attributes; preserves the "one service per attribute" rule.
- **Combined extension name** stays as `AddNuonDependencyInjectionServices`.
- **Code fixes** ship as separate `Nuons.{Area}.CodeFixes` projects (and matching test projects), matching the Microsoft analyzer-with-code-fix template layout.
- **Multi-interface registration**: stays as today — one (service, impl) per attribute. No `AsImplementedInterfaces` mode.

---

## Phase 0 — Must-fix correctness bugs

These are defects, not features. They block v1 regardless of feature scope.

### Code Injection

- [x] **NUCI-bug-1** Match `[Injected]` and `[InjectedOptions]` by **metadata name**, not short class name. Today [KnownCodeInjectionTypes.cs:6-7](src/Nuons.CodeInjection.Generators/KnownCodeInjectionTypes.cs#L6-L7) stores short names and [InjectConstructorGenerator.cs:52,60](src/Nuons.CodeInjection.Generators/InjectConstructorGenerator.cs#L52-L60) compares `AttributeClass.Name` — any unrelated `InjectedAttribute` in any namespace currently triggers it.
- [ ] **NUCI-bug-2** Support generic classes. [InjectionSourceBuilder.cs:40](src/Nuons.CodeInjection.Generators/InjectionSourceBuilder.cs#L40) emits `partial class {ClassName}` with no type parameters.
- [ ] **NUCI-bug-3** Support nested classes. Today [InjectConstructorGenerator.cs:30-34](src/Nuons.CodeInjection.Generators/InjectConstructorGenerator.cs#L30-L34) computes namespace-only and the builder emits a flat partial — won't compile inside an outer type.
- [ ] **NUCI-bug-4** Decide policy for classes in the global namespace (currently silently skipped at [InjectConstructorGenerator.cs:30-34](src/Nuons.CodeInjection.Generators/InjectConstructorGenerator.cs#L30-L34)). Either support, or emit a diagnostic instead of silence.
- [ ] **NUCI-bug-5** Stop dropping accessibility modifiers in [InjectConstructorGenerator.cs:75-83](src/Nuons.CodeInjection.Generators/InjectConstructorGenerator.cs#L75-L83). Simplest fix: emit the partial with no accessibility modifier (legal on partials, inherited from the user's declaration).
- [ ] **NUCI-bug-6** Disambiguate generated hint files. [InjectConstructorGenerator.cs:93](src/Nuons.CodeInjection.Generators/InjectConstructorGenerator.cs#L93) uses `{ClassName}.g.cs` — two `Foo` in different namespaces collide. Include namespace (or a hash) in the hint name.
- [ ] **NUCI-bug-7** Define behaviour when `[InjectConstructor]` finds zero `[Injected]` fields. Today returns null and emits nothing ([InjectConstructorGenerator.cs:65-68](src/Nuons.CodeInjection.Generators/InjectConstructorGenerator.cs#L65-L68)). Emit empty `public Foo() { }` *or* surface an analyzer diagnostic — pick one.

### Dependency Injection

- [x] **NUDI-bug-1** `Sources.TrimForClassName` ([Sources.cs:15-16](src/Nuons.Core.Generators/Sources.cs#L15-L16)) collapses `My.Lib` and `MyLib` to the same class name and the same hint. Add a uniqueness suffix (hash of full assembly name).
- [x] **NUDI-bug-2** [DependencyInjectionAnalyzerContext.cs:12-15](src/Nuons.DependencyInjection.Analyzers/DependencyInjectionAnalyzerContext.cs#L12-L15) only registers the non-generic attributes (TODO already present). NUDI001 misses combos like `[Singleton][Singleton<IFoo>]`. Add the generic variants by metadata name.
- [x] **NUDI-bug-3** `Lifetime` enum is `public` ([Lifetime.cs:3](src/Nuons.DependencyInjection.Generators/Lifetime.cs#L3)) but ships in a generator-only DLL. Make `internal`.

### Conventions / hardening

- [x] **conv-1** Make `[DiagnosticAnalyzer]` types `public sealed` (currently `internal`) to align with Roslyn convention. Affects all four analyzers.
- [x] **conv-2** Add an XML comment on `OptionsAttribute` ([OptionsAttribute.cs](src/Nuons.DependencyInjection.Abstractions/OptionsAttribute.cs)) noting the constructor parameter is read by the generator via `ConstructorArguments` — the attribute body is intentionally empty.
- [x] **conv-3** Document the invariant in [CombinedServiceRegistrationSourceBuilder.cs:29-31](src/Nuons.DependencyInjection.Generators.Startup/CombinedServiceRegistrationSourceBuilder.cs#L29-L31) that `OptionsRegistration{Asm}` is always emitted (even empty) so the combined call compiles. Add a comment on both producer and consumer; revisit under "future".

---

## Phase 1 — Required v1 features

### Options validation

Add to `OptionsAttribute` (additive — existing one-string-arg constructor stays valid):

```csharp
public class OptionsAttribute(string sectionKey) : Attribute
{
	public bool Validate { get; init; }
	public bool ValidateOnStart { get; init; }
}
```

Generator emits:

| `Validate` | `ValidateOnStart` | Generated registration |
|---|---|---|
| `false` | `false` | `services.Configure<T>(section)` *(today's behaviour)* |
| `true`  | `false` | `services.AddOptions<T>().Bind(section).ValidateDataAnnotations()` |
| `false` | `true`  | `services.AddOptions<T>().Bind(section).ValidateOnStart()` |
| `true`  | `true`  | `services.AddOptions<T>().Bind(section).ValidateDataAnnotations().ValidateOnStart()` |

Notes:

- `ValidateOnStart` requires `Microsoft.Extensions.Hosting`. Keep the flags independent so projects without hosting can still get DataAnnotations validation.
- `OptionsRegistration{Asm}` continues to expose the same `ConfigureOptions(IServiceCollection, IConfiguration)` surface — only the body changes shape per registration.

- [x] Extend [OptionsAttribute.cs](src/Nuons.DependencyInjection.Abstractions/OptionsAttribute.cs) with `Validate` and `ValidateOnStart` properties.
- [x] Extend [OptionsRegistration.cs](src/Nuons.DependencyInjection.Generators/OptionsRegistration.cs) and the source builder to emit the four variants above.
- [x] Extend caching tests in [OptionsRegistrationGeneratorCachingTests.cs](tests/Nuons.DependencyInjection.Generators.Tests/OptionsRegistrationGeneratorCachingTests.cs) to cover flag changes.
- [x] New verified snapshots covering each flag combination.

### Code fixes for existing analyzers

- [x] **NUCI001 fix** add the `partial` modifier to a class with `[InjectConstructor]`.
- [x] **NUCI002 fix** add `[InjectConstructor]` to a class with `[Injected]`/`[InjectedOptions]` fields.

(NUDI001 and NUDI002 are intentionally not auto-fixable — choosing which attribute or which interface to keep is user intent.)

### Project layout additions

- [x] `src/Nuons.DependencyInjection.CodeFixes/` (netstandard2.0)
- [x] `src/Nuons.CodeInjection.CodeFixes/` (netstandard2.0)
- [x] `tests/Nuons.DependencyInjection.CodeFixes.Tests/`
- [x] `tests/Nuons.CodeInjection.CodeFixes.Tests/`
- [x] Wire DLLs into the `Nuons` package under `analyzers/dotnet/cs/` (parallel to the existing analyzer DLLs).

---

## Phase 2 — Strongly recommended for v1

These are the analyzers that catch real-world misuses early. Each has a clear diagnostic and a clear fix where applicable.

| ID | Title | Severity | Code fix |
|---|---|---|---|
| **NUCI003** | `[InjectedOptions]` field typed as `IOptions<T>` (would generate `IOptions<IOptions<T>>`) | Error | Replace with `T` |
| **NUCI004** | `[InjectConstructor]` class has no `[Injected]` fields | Info | — |
| **NUCI005** | `[InjectConstructor]` class already declares a constructor (would compile-fail on duplicate ctor) | Error | — |
| **NUCI006** | `[Injected]` field is not `readonly` | Warning | Add `readonly` |
| **NUDI003** | `[Singleton<TService>]` (or scoped/transient) where `TImpl` does not implement/inherit `TService` | Error | — |
| **NUDI004** | Section key on `[Options(...)]` is empty or whitespace | Warning | — |

- [ ] Implement analyzers above; one diagnostic per file, mirror the existing pattern.
- [ ] Implement code fixes where listed.
- [ ] Add test files following the existing `Nuons.DependencyInjection.Analyzers.Tests` / `Nuons.CodeInjection.Analyzers.Tests` shape.

### Symbol-action conversion (cleanup)

- [ ] Convert the two existing DI analyzers from `RegisterSyntaxNodeAction(ClassDeclaration)` to `RegisterSymbolAction(SymbolKind.NamedType)`. Same behaviour, runs once per symbol regardless of partial declarations, and makes the new NUDI003 cleaner to share helpers.

---

## Phase 3 — Decide before v1 ships (depends on complexity)

Items the user wants in the plan but will skip if implementation cost is too high. Each gets a feasibility spike before commitment.

### Keyed services

```csharp
[Singleton(Key = "primary")]
public class PrimaryFoo : IFoo;

[Singleton<IFoo>(Key = "secondary")]
public class SecondaryFoo : IFoo;
```

Generator emits `services.AddKeyedSingleton<IFoo, PrimaryFoo>("primary")`. Touches:

- [ ] Add `Key` property on each of [SingletonAttribute.cs](src/Nuons.DependencyInjection.Abstractions/SingletonAttribute.cs), [ScopedAttribute.cs](src/Nuons.DependencyInjection.Abstractions/ScopedAttribute.cs), [TransientAttribute.cs](src/Nuons.DependencyInjection.Abstractions/TransientAttribute.cs) (both generic and non-generic).
- [ ] Extend [ServiceRegistration.cs](src/Nuons.DependencyInjection.Generators/ServiceRegistration.cs) with `Key` and the source builder to switch between `AddX<,>()` / `AddKeyedX<,>(key)`.
- [ ] Caching tests for key changes.
- [ ] Decide: do `[InjectedOptions]` / `[Injected]` fields support a key? Probably yes via `[Injected(Key = "...")]` so injection flows can match — but this couples Code Injection to DI semantics. **Open question**: include keyed support in CodeInjection v1 or only on the registration side?

### Generic / nested `[InjectConstructor]`

These are listed under Phase 0 (NUCI-bug-2, NUCI-bug-3) because they are correctness fixes for documented usage. **However**, full nested-class support (multiple levels, generic outer types) can be expensive — fall back to:

- [ ] Top-level generic classes: in scope.
- [ ] Single-level nested classes: in scope.
- [ ] Multi-level nested + generic outer: emit a clear diagnostic and skip generation if the cost is high.

### Captive-dependency analyzer

- [ ] **NUDI005** Singleton service has an `[Injected]` field whose type is registered as `Scoped` or `Transient`. Cross-cuts CodeInjection + DI; needs both attribute trees visible to the analyzer. Severity: warning. **Skip if complexity grows** — this can ship in v1.x.

### `TryAdd*` registration

- [ ] Property on the lifetime attributes: `TryAdd = true` → emit `services.TryAdd{Lifetime}<,>()`. Useful for libraries that want defaults overridable by consumers. Low implementation cost; bundling with keyed support makes sense.

---

## Phase 4 — Future / noted only

Tracked, not planned for v1.

- **Property injection** (`[Injected]` on an `init`-only property). Lower priority; document as unsupported in v1.
- **Options discovery for the combiner**: replace the "always emit empty `OptionsRegistration{Asm}`" invariant with a discovery model where the combiner enumerates which classes the per-assembly generator actually produced. Investigate if this can be done efficiently inside an `IIncrementalGenerator` pipeline — current design is intentionally simple.
- **Open generic registrations**: `services.AddSingleton(typeof(IRepo<>), typeof(Repo<>))` — needs a different attribute shape because the type-parameter slot can't be open in C# attribute generic args.
- **Multiple service interfaces per impl** (`AsImplementedInterfaces`-style): explicitly out of scope per decision above.
- **Decorators / interception**: out of scope.
- **Factory method registration** (`AddSingleton<T>(sp => ...)`): out of scope; users can write `services.AddSingleton<T>(...)` directly alongside `AddNuonDependencyInjectionServices`.
- **Named (non-keyed) options**: `services.Configure<T>(name, section)` — out of scope.
- **Base-class constructor chaining** in `[InjectConstructor]` — when base class has its own injected ctor, forward parameters via `: base(...)`. Future work.
- **Field-name → parameter-name policy** (e.g., strip `_` prefix). Currently identical; revisit if user feedback demands it.

---

## Packaging note

User has indicated `PrivateAssets="all"` should no longer be required. This is research-first, not a commitment:

- [ ] Confirm whether the Roslyn analyzer DLL flow-on rules in current SDKs no longer leak generator references downstream. If so, drop the `PrivateAssets="all"` requirement from the README and any sample project files.
- [ ] If still required, document the exact reason (which DLL flows where) so users get a one-line `<PackageReference ... PrivateAssets="all" />` snippet in the README.
- [ ] No changes to the `lib/netstandard2.0` / `analyzers/dotnet/cs/` split. Treat the current `Nuons` + `Nuons.Startup` two-package layout as fixed for v1.

---

## Documentation updates

When the implementation lands:

- [ ] Update [nuons-dependency-injection.md](documentation/nuons-dependency-injection.md): document `Validate` / `ValidateOnStart` / `Key` / `TryAdd` properties and the resolution rules (when `TImpl` is used vs sole interface vs generic arg). Spell out runtime requirements (`Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Options`, optionally `Microsoft.Extensions.Hosting` for `ValidateOnStart`).
- [ ] Update [nuons-code-injection.md](documentation/nuons-code-injection.md): document the `null!` field-initialiser convention, supported class shapes (top-level, generic, single-level nested), and the analyzers' diagnostic IDs.
- [ ] Cross-reference NUCI / NUDI codes from a single "diagnostics" table in one of the docs — easier for users to look up.

---

## Implementation order (suggested)

1. **Phase 0** in one sweep — pure correctness, no design debate.
2. **Phase 1** options validation, then code fixes for NUCI001/NUCI002. Ship as a v1-preview at this point if useful.
3. **Phase 2** new analyzers + symbol-action cleanup. Each analyzer is independent — parallelisable.
4. **Phase 3** keyed + `TryAdd` together (similar shape on the attribute and emitter); generic/nested `[InjectConstructor]` next; captive-dependency analyzer last (can defer).
5. **Phase 4** triaged on user feedback after v1 releases.
