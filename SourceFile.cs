namespace DSPtoVCXPROJ;

/// <summary>
/// A source file, meant to contain all the objects of the file for all configurations.
/// </summary>
class SourceFile
{
	/// <summary>
	/// The configurations for this source file, a dictionary whose key is the platform and configuration and whose value is data of any
	/// class deriving from <see cref="None" />.
	/// </summary>
	public Dictionary<string, None> Configurations { get; init; } = [];
}
