namespace DSPtoVCXPROJ;

/// <summary>
/// A wrapper around multiple <see cref="ClCompile" /> objects, to allow all of them to set the same property all at once.
/// </summary>
class MultipleClCompile
{
	public List<ClCompile> Objects { get; set; } = new();

	public bool? ExcludedFromBuild
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.ExcludedFromBuild = value;
		}
	}

	public void AdditionalIncludeDirectoriesAdd(string value)
	{
		foreach (var obj in this.Objects)
			_ = obj.AdditionalIncludeDirectories.Add(value);
	}

	public string? DebugInformationFormat
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.DebugInformationFormat = value;
		}
	}

	public bool? SuppressStartupBanner
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.SuppressStartupBanner = value;
		}
	}

	public string? WarningLevel
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.WarningLevel = value;
		}
	}

	public bool? TreatWarningAsError
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.TreatWarningAsError = value;
		}
	}

	public string? Optimization
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.Optimization = value;
		}
	}

	public string? InlineFunctionExpansion
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.InlineFunctionExpansion = value;
		}
	}

	public bool? IntrinsicFunctions
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.IntrinsicFunctions = value;
		}
	}

	public string? FavorSizeOrSpeed
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.FavorSizeOrSpeed = value;
		}
	}

	public bool? OmitFramePointers
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.OmitFramePointers = value;
		}
	}

	public bool? EnableFiberSafeOptimizations
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.EnableFiberSafeOptimizations = value;
		}
	}

	public void PreprocessorDefinitionsAdd(string value)
	{
		foreach (var obj in this.Objects)
			_ = obj.PreprocessorDefinitions.Add(value);
	}

	public void UndefinePreprocessorDefinitionsAdd(string value)
	{
		foreach (var obj in this.Objects)
			_ = obj.UndefinePreprocessorDefinitions.Add(value);
	}

	public bool? UndefineAllPreprocessorDefinitions
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.UndefineAllPreprocessorDefinitions = value;
		}
	}

	public bool? IgnoreStandardIncludePath
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.IgnoreStandardIncludePath = value;
		}
	}

	public bool? PreprocessToFile
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.PreprocessToFile = value;
		}
	}

	public bool? PreprocessSuppressLineNumbers
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.PreprocessSuppressLineNumbers = value;
		}
	}

	public bool? PreprocessKeepComments
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.PreprocessKeepComments = value;
		}
	}

	public bool? StringPooling
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.StringPooling = value;
		}
	}

	public string? ExceptionHandling
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.ExceptionHandling = value;
		}
	}

	public string? BasicRuntimeChecks
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.BasicRuntimeChecks = value;
		}
	}

	public string? RuntimeLibrary
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.RuntimeLibrary = value;
		}
	}

	public string? StructMemberAlignment
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.StructMemberAlignment = value;
		}
	}

	public bool? FunctionLevelLinking
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.FunctionLevelLinking = value;
		}
	}

	public bool? DisableLanguageExtensions
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.DisableLanguageExtensions = value;
		}
	}

	public bool? RuntimeTypeInfo
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.RuntimeTypeInfo = value;
		}
	}

	public string? PrecompiledHeader
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.PrecompiledHeader = value;
		}
	}

	public string? PrecompiledHeaderFile
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.PrecompiledHeaderFile = value;
		}
	}

	public string? PrecompiledHeaderOutputFile
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.PrecompiledHeaderOutputFile = value;
		}
	}

	public string? AssemblerOutput
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.AssemblerOutput = value;
		}
	}

	public string? AssemblerListingLocation
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.AssemblerListingLocation = value;
		}
	}

	public string? ObjectFileName
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.ObjectFileName = value;
		}
	}

	public string? ProgramDataBaseFileName
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.ProgramDataBaseFileName = value;
		}
	}

	public bool? BrowseInformation
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.BrowseInformation = value;
		}
	}

	public string? BrowseInformationFile
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.BrowseInformationFile = value;
		}
	}

	public string? CallingConvention
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.CallingConvention = value;
		}
	}

	public string? CompileAs
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.CompileAs = value;
		}
	}

	public void ForcedIncludeFilesAdd(string value)
	{
		foreach (var obj in this.Objects)
			_ = obj.ForcedIncludeFiles.Add(value);
	}

	public bool? OmitDefaultLibName
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.OmitDefaultLibName = value;
		}
	}

	public void AdditionalOptionsAdd(string value)
	{
		foreach (var obj in this.Objects)
			_ = obj.AdditionalOptions.Add(value);
	}
}
