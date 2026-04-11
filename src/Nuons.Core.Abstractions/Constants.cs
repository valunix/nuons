namespace Nuons.Core.Abstractions;

/// <summary>
/// Shared constants used across the Nuons library.
/// </summary>
public static class Constants
{
	/// <summary>
	/// The conditional compilation symbol that controls whether Nuons code-generation attributes are emitted into the compiled output.
	/// Define <c>NuonsCodeGeneration</c> in your project to include the attributes; omit it (the default) to have them stripped at compile time.
	/// </summary>
	public const string CodeGenerationCondition = "NuonsCodeGeneration";
}
