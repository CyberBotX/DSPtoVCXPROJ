using System.Xml.Linq;

namespace DSPtoVCXPROJ;

/// <summary>
/// The ResourceCompile block contains the information for Resources properties of either the entire project or a specific RC file.
/// </summary>
class ResourceCompile : CompileBase
{
	public ResourceCompile() : base()
	{
	}

	public ResourceCompile(string include, string condition) : base(include, condition)
	{
	}

	// Order is based on how Visual Studio 2022 has them in its properties window
	public HashSet<string> PreprocessorDefinitions { get; } = new();
	public HashSet<string> UndefinePreprocessorDefinitions { get; } = new();
	public string? Culture { get; set; }
	public HashSet<string> AdditionalIncludeDirectories { get; } = new();
	public bool? IgnoreStandardIncludePath { get; set; }
	public bool? ShowProgress { get; set; }
	public string? ResourceOutputFileName { get; set; }
	public bool? NullTerminateStrings { get; set; }
	public HashSet<string> AdditionalOptions { get; } = new();

	public override XElement GetBlock()
	{
		var block = base.GetBlock();
		block.Name = Program.NS + nameof(ResourceCompile);
		// This condition is so the same ResourceCompile object can be used for both creating an entry in the filters file as well as the
		// project file.
		if (this.Filter is null)
		{
			this.AddIfNotEmpty(block, this.PreprocessorDefinitions, ';');
			this.AddIfNotEmpty(block, this.UndefinePreprocessorDefinitions, ';');
			this.AddIfNotBlank(block, this.Culture);
			this.AddIfNotEmpty(block, this.AdditionalIncludeDirectories, ';');
			this.AddIfNotNull(block, this.IgnoreStandardIncludePath);
			this.AddIfNotNull(block, this.ShowProgress);
			this.AddIfNotBlank(block, this.ResourceOutputFileName);
			this.AddIfNotNull(block, this.NullTerminateStrings);
			this.AddIfNotEmpty(block, this.AdditionalOptions, ' ');
		}
		return block;
	}
}
