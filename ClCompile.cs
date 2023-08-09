using System.Xml.Linq;

namespace DSPtoVCXPROJ;

/// <summary>
/// The <see cref="ClCompile" /> block contains the information for the C/C++ properties of either the entire project or a specific C/C++
/// source file.
/// </summary>
class ClCompile : CompileBase
{
	public ClCompile() : base()
	{
	}

	public ClCompile(string include, string condition) : base(include, condition)
	{
	}

	// Order is based on how Visual Studio 2022 has them in its properties window
	public bool? ExcludedFromBuild { get; set; }
	public HashSet<string> AdditionalIncludeDirectories { get; } = new();
	public string? DebugInformationFormat { get; set; }
	public bool? SuppressStartupBanner { get; set; }
	public string? WarningLevel { get; set; }
	public bool? TreatWarningAsError { get; set; }
	public string? Optimization { get; set; }
	public string? InlineFunctionExpansion { get; set; }
	public bool? IntrinsicFunctions { get; set; }
	public string? FavorSizeOrSpeed { get; set; }
	public bool? OmitFramePointers { get; set; }
	public bool? EnableFiberSafeOptimizations { get; set; }
	public HashSet<string> PreprocessorDefinitions { get; } = new();
	public HashSet<string> UndefinePreprocessorDefinitions { get; } = new();
	public bool? UndefineAllPreprocessorDefinitions { get; set; }
	public bool? IgnoreStandardIncludePath { get; set; }
	public bool? PreprocessToFile { get; set; }
	public bool? PreprocessSuppressLineNumbers { get; set; }
	public bool? PreprocessKeepComments { get; set; }
	public bool? StringPooling { get; set; }
	public string? ExceptionHandling { get; set; }
	public string? BasicRuntimeChecks { get; set; }
	public string? RuntimeLibrary { get; set; }
	public string? StructMemberAlignment { get; set; }
	public bool? FunctionLevelLinking { get; set; }
	public bool? DisableLanguageExtensions { get; set; }
	public bool? RuntimeTypeInfo { get; set; }
	public string? PrecompiledHeader { get; set; }
	public string? PrecompiledHeaderFile { get; set; }
	public string? PrecompiledHeaderOutputFile { get; set; }
	public string? AssemblerOutput { get; set; }
	public string? AssemblerListingLocation { get; set; }
	public string? ObjectFileName { get; set; }
	public string? ProgramDataBaseFileName { get; set; }
	public bool? BrowseInformation { get; set; }
	public string? BrowseInformationFile { get; set; }
	public string? CallingConvention { get; set; }
	public string? CompileAs { get; set; }
	public HashSet<string> ForcedIncludeFiles { get; } = new();
	public bool? OmitDefaultLibName { get; set; }
	public HashSet<string> AdditionalOptions { get; } = new();

	public override XElement GetBlock()
	{
		var block = base.GetBlock();
		block.Name = Program.NS + nameof(ClCompile);
		// This condition is so the same ClCompile object can be used for both creating an entry in the filters file as well as the project
		// file.
		if (this.Filter is null)
		{
			this.AddIfNotNull(block, this.ExcludedFromBuild);
			this.AddIfNotEmpty(block, this.AdditionalIncludeDirectories, ';');
			this.AddIfNotBlank(block, this.DebugInformationFormat);
			this.AddIfNotNull(block, this.SuppressStartupBanner);
			this.AddIfNotBlank(block, this.WarningLevel);
			this.AddIfNotNull(block, this.TreatWarningAsError);
			this.AddIfNotBlank(block, this.Optimization);
			this.AddIfNotBlank(block, this.InlineFunctionExpansion);
			this.AddIfNotNull(block, this.IntrinsicFunctions);
			this.AddIfNotBlank(block, this.FavorSizeOrSpeed);
			this.AddIfNotNull(block, this.OmitFramePointers);
			this.AddIfNotNull(block, this.EnableFiberSafeOptimizations);
			this.AddIfNotEmpty(block, this.PreprocessorDefinitions, ';');
			this.AddIfNotEmpty(block, this.UndefinePreprocessorDefinitions, ';');
			this.AddIfNotNull(block, this.UndefineAllPreprocessorDefinitions);
			this.AddIfNotNull(block, this.IgnoreStandardIncludePath);
			this.AddIfNotNull(block, this.PreprocessToFile);
			this.AddIfNotNull(block, this.PreprocessSuppressLineNumbers);
			this.AddIfNotNull(block, this.PreprocessKeepComments);
			this.AddIfNotNull(block, this.StringPooling);
			this.AddIfNotBlank(block, this.ExceptionHandling);
			this.AddIfNotBlank(block, this.BasicRuntimeChecks);
			this.AddIfNotBlank(block, this.RuntimeLibrary);
			this.AddIfNotBlank(block, this.StructMemberAlignment);
			this.AddIfNotNull(block, this.FunctionLevelLinking);
			this.AddIfNotNull(block, this.DisableLanguageExtensions);
			this.AddIfNotNull(block, this.RuntimeTypeInfo);
			this.AddIfNotBlank(block, this.PrecompiledHeader);
			this.AddIfNotBlank(block, this.PrecompiledHeaderFile);
			this.AddIfNotBlank(block, this.PrecompiledHeaderOutputFile);
			this.AddIfNotBlank(block, this.AssemblerOutput);
			this.AddIfNotBlank(block, this.AssemblerListingLocation);
			this.AddIfNotBlank(block, this.ObjectFileName);
			this.AddIfNotBlank(block, this.ProgramDataBaseFileName);
			this.AddIfNotNull(block, this.BrowseInformation);
			this.AddIfNotBlank(block, this.BrowseInformationFile);
			this.AddIfNotBlank(block, this.CallingConvention);
			this.AddIfNotBlank(block, this.CompileAs);
			this.AddIfNotEmpty(block, this.ForcedIncludeFiles, ';');
			this.AddIfNotNull(block, this.OmitDefaultLibName);
			this.AddIfNotEmpty(block, this.AdditionalOptions, ' ');
		}
		return block;
	}
}
