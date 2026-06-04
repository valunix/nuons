namespace Nuons.DependencyInjection.Generators;

internal record OptionsRegistration(string SectionKey, string ClassName, bool Validate, bool ValidateOnStart);
