using System.Xml.Linq;

namespace DSPtoVCXPROJ;

/// <summary>
/// The <see cref="Lib" /> block contains the information for the Librarian properties of the entire project.
/// </summary>
class Lib : None
{
	// Order is based on how Visual Studio 2022 has them in its properties window
	public string? OutputFile { get; set; }
	public HashSet<string> AdditionalDependencies { get; } = [];
	public HashSet<string> AdditionalLibraryDirectories { get; } = [];
	public bool? SuppressStartupBanner { get; set; }
	public string? ModuleDefinitionFile { get; set; }
	public bool? IgnoreAllDefaultLibraries { get; set; }
	public HashSet<string> IgnoreSpecificDefaultLibraries { get; } = [];
	public HashSet<string> ExportNamedFunctions { get; } = [];
	public string? ForceSymbolReferences { get; set; }
	public string? TargetMachine { get; set; }
	public string? SubSystem { get; set; }
	public HashSet<string> RemoveObjects { get; } = [];
	public bool? Verbose { get; set; }
	public string? Name { get; set; }
	public HashSet<string> AdditionalOptions { get; } = [];

	public override XElement GetBlock()
	{
		var block = base.GetBlock();
		block.Name = Program.NS + nameof(Lib);
		this.AddIfNotBlank(block, this.OutputFile);
		if (this.IgnoreAllDefaultLibraries != true)
			_ = this.AdditionalDependencies.RemoveWhere(Link.InheritedAdditionalDependencies.Contains);
		this.AddIfNotEmpty(block, this.AdditionalDependencies, ';');
		this.AddIfNotEmpty(block, this.AdditionalLibraryDirectories, ';');
		this.AddIfNotNull(block, this.SuppressStartupBanner);
		this.AddIfNotBlank(block, this.ModuleDefinitionFile);
		this.AddIfNotNull(block, this.IgnoreAllDefaultLibraries);
		this.AddIfNotEmpty(block, this.IgnoreSpecificDefaultLibraries, ';');
		this.AddIfNotEmpty(block, this.ExportNamedFunctions, ';');
		this.AddIfNotBlank(block, this.ForceSymbolReferences);
		this.AddIfNotBlank(block, this.TargetMachine);
		this.AddIfNotBlank(block, this.SubSystem);
		this.AddIfNotEmpty(block, this.RemoveObjects, ';');
		this.AddIfNotNull(block, this.Verbose);
		this.AddIfNotBlank(block, this.Name);
		this.AddIfNotEmpty(block, this.AdditionalOptions, ' ');
		return block;
	}
}
