namespace DSPtoVCXPROJ;

/// <summary>
/// A wrapper around multiple <see cref="ResourceCompile" /> objects, to allow all of them to set the same property all at once.
/// </summary>
class MultipleResourceCompile
{
	public List<ResourceCompile> Objects { get; set; } = [];

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

	public string? Culture
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.Culture = value;
		}
	}

	public void AdditionalIncludeDirectoriesAdd(string value)
	{
		foreach (var obj in this.Objects)
			_ = obj.AdditionalIncludeDirectories.Add(value);
	}

	public bool? IgnoreStandardIncludePath
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.IgnoreStandardIncludePath = value;
		}
	}

	public bool? ShowProgress
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.ShowProgress = value;
		}
	}

	public string? ResourceOutputFileName
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.ResourceOutputFileName = value;
		}
	}

	public bool? NullTerminateStrings
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.NullTerminateStrings = value;
		}
	}

	public void AdditionalOptionsAdd(string value)
	{
		foreach (var obj in this.Objects)
			_ = obj.AdditionalOptions.Add(value);
	}
}
