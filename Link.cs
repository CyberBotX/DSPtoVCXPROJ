using System.Xml.Linq;

namespace DSPtoVCXPROJ;

/// <summary>
/// The <see cref="Link" /> block contains information for the Linker properties of the entire project.
/// </summary>
class Link : None
{
	internal static readonly HashSet<string> InheritedAdditionalDependencies = new()
	{
		"kernel32.lib",
		"user32.lib",
		"gdi32.lib",
		"winspool.lib",
		"comdlg32.lib",
		"advapi32.lib",
		"shell32.lib",
		"ole32.lib",
		"oleaut32.lib",
		"uuid.lib",
		"odbc32.lib",
		"odbccp32.lib"
	};

	// Order is based on how Visual Studio 2022 has them in its properties window
	public string? OutputFile { get; set; }
	public string? ShowProgress { get; set; }
	public string? Version { get; set; }
	public bool? LinkIncremental { get; set; }
	public bool? SuppressStartupBanner { get; set; }
	public HashSet<string> AdditionalLibraryDirectories { get; } = new();
	public string? ForceFileOutput { get; set; }
	public string? SpecifySectionAttributes { get; set; }
	public HashSet<string> AdditionalDependencies { get; } = new();
	public bool? IgnoreAllDefaultLibraries { get; set; }
	public HashSet<string> IgnoreSpecificDefaultLibraries { get; } = new();
	public string? ModuleDefinitionFile { get; set; }
	public HashSet<string> ForceSymbolReferences { get; } = new();
	public HashSet<string> DelayLoadDLLs { get; } = new();
	public bool? GenerateDebugInformation { get; set; }
	public string? ProgramDatabaseFile { get; set; }
	public bool? GenerateMapFile { get; set; }
	public string? MapFileName { get; set; }
	public bool? MapExports { get; set; }
	public string? SubSystem { get; set; }
	public string? HeapReserveSize { get; set; }
	public string? HeapCommitSize { get; set; }
	public string? StackReserveSize { get; set; }
	public string? StackCommitSize { get; set; }
	public bool? LargeAddressAware { get; set; }
	public bool? SwapRunFromCD { get; set; }
	public bool? SwapRunFromNET { get; set; }
	public string? Driver { get; set; }
	public bool? OptimizeReferences { get; set; }
	public bool? EnableCOMDATFolding { get; set; }
	public string? FunctionOrder { get; set; }
	public string? EntryPointSymbol { get; set; }
	public bool? NoEntryPoint { get; set; }
	public bool? SetChecksum { get; set; }
	public string? BaseAddress { get; set; }
	public bool? FixedBaseAddress { get; set; }
	public bool? SupportUnloadOfDelayLoadedDLL { get; set; }
	public bool? SupportNobindOfDelayLoadedDLL { get; set; }
	public string? ImportLibrary { get; set; }
	public string? MergeSections { get; set; }
	public string? TargetMachine { get; set; }
	public bool? Profile { get; set; }
	public string? SectionAlignment { get; set; }
	// This is the only option here that has no entry in the property pages but does have one in MSBuild
	public string? MSDOSStubFileName { get; set; }
	public HashSet<string> AdditionalOptions { get; } = new();

	public override XElement GetBlock()
	{
		var block = base.GetBlock();
		block.Name = Program.NS + nameof(Link);
		this.AddIfNotBlank(block, this.OutputFile);
		this.AddIfNotBlank(block, this.ShowProgress);
		this.AddIfNotBlank(block, this.Version);
		this.AddIfNotNull(block, this.LinkIncremental);
		this.AddIfNotNull(block, this.SuppressStartupBanner);
		this.AddIfNotEmpty(block, this.AdditionalLibraryDirectories, ';');
		this.AddIfNotBlank(block, this.ForceFileOutput);
		this.AddIfNotBlank(block, this.SpecifySectionAttributes);
		if (this.IgnoreAllDefaultLibraries != true)
			_ = this.AdditionalDependencies.RemoveWhere(Link.InheritedAdditionalDependencies.Contains);
		this.AddIfNotEmpty(block, this.AdditionalDependencies, ';');
		this.AddIfNotNull(block, this.IgnoreAllDefaultLibraries);
		this.AddIfNotEmpty(block, this.IgnoreSpecificDefaultLibraries, ';');
		this.AddIfNotBlank(block, this.ModuleDefinitionFile);
		this.AddIfNotEmpty(block, this.ForceSymbolReferences, ';');
		this.AddIfNotEmpty(block, this.DelayLoadDLLs, ';');
		this.AddIfNotNull(block, this.GenerateDebugInformation);
		this.AddIfNotBlank(block, this.ProgramDatabaseFile);
		this.AddIfNotNull(block, this.GenerateMapFile);
		this.AddIfNotBlank(block, this.MapFileName);
		this.AddIfNotNull(block, this.MapExports);
		this.AddIfNotBlank(block, this.SubSystem);
		this.AddIfNotBlank(block, this.HeapReserveSize);
		this.AddIfNotBlank(block, this.HeapCommitSize);
		this.AddIfNotBlank(block, this.StackReserveSize);
		this.AddIfNotBlank(block, this.StackCommitSize);
		this.AddIfNotNull(block, this.LargeAddressAware);
		this.AddIfNotNull(block, this.SwapRunFromCD);
		this.AddIfNotNull(block, this.SwapRunFromNET);
		this.AddIfNotBlank(block, this.Driver);
		this.AddIfNotNull(block, this.OptimizeReferences);
		this.AddIfNotNull(block, this.EnableCOMDATFolding);
		this.AddIfNotBlank(block, this.FunctionOrder);
		this.AddIfNotBlank(block, this.EntryPointSymbol);
		this.AddIfNotNull(block, this.NoEntryPoint);
		this.AddIfNotNull(block, this.SetChecksum);
		this.AddIfNotBlank(block, this.BaseAddress);
		this.AddIfNotNull(block, this.FixedBaseAddress);
		this.AddIfNotNull(block, this.SupportUnloadOfDelayLoadedDLL);
		this.AddIfNotNull(block, this.SupportNobindOfDelayLoadedDLL);
		this.AddIfNotBlank(block, this.ImportLibrary);
		this.AddIfNotBlank(block, this.MergeSections);
		this.AddIfNotBlank(block, this.TargetMachine);
		this.AddIfNotNull(block, this.Profile);
		this.AddIfNotBlank(block, this.SectionAlignment);
		this.AddIfNotBlank(block, this.MSDOSStubFileName);
		this.AddIfNotEmpty(block, this.AdditionalOptions, ' ');
		return block;
	}
}
