using System.Xml.Linq;

namespace DSPtoVCXPROJ;

/// <summary>
/// The configuration properties of a project, the condition is usually a comparison against Configuration and Platform.
/// </summary>
class ConfigurationProperties
{
	public string Condition { get; }

	public ConfigurationProperties(string condition) => this.Condition = condition;

	public bool? UseDebugLibraries { get; set; }
	public string? UseOfMfc { get; set; }
	public string? CharacterSet { get; set; }
	public string? OutDir { get; set; }
	public string? IntDir { get; set; }

	public ClCompile ClCompile { get; } = new();
	public Link Link { get; } = new();
	public Lib Lib { get; } = new();
	public ResourceCompile ResourceCompile { get; } = new();
	public PreLinkEvent PreLinkEvent { get; } = new();
	public PostBuildEvent PostBuildEvent { get; } = new();
	public CustomBuildStep CustomBuildStep { get; } = new();

	/// <summary>
	/// Gets the PropertyGroup block for before the PropertySheets blocks.
	/// </summary>
	/// <returns>The PropertyGroup <see cref="XElement" /> block.</returns>
	public XElement GetPrePropsBlock()
	{
		var block = Program.CreatePropertyGroupBlock(this.Condition, "Configuration");
		block.AddIfNotBlank(Program.ConfigurationType);
		block.AddIfNotNull(this.UseDebugLibraries);
		block.AddIfNotBlank(this.UseOfMfc);
		block.AddIfNotBlank(this.CharacterSet);
		return block;
	}

	/// <summary>
	/// Gets the PropertyGroup block for after the PropertySheets blocks.
	/// </summary>
	/// <returns>The PropertyGroup <see cref="XElement" /> block.</returns>
	public XElement GetPostPropsBlock()
	{
		var block = Program.CreatePropertyGroupBlock(this.Condition, null);
		block.AddIfNotBlank(this.OutDir);
		block.AddIfNotBlank(this.IntDir);
		return block;
	}

	/// <summary>
	/// Gets the ItemDefinitionGroup block for after the post-PropertySheets PropertyGroup block.
	/// </summary>
	/// <returns>The PropertyGroup <see cref="XElement" /> block.</returns>
	public XElement GetItemDefinitionBlock()
	{
		var block = Program.CreateElement("ItemDefinitionGroup");
		block.AddAttributeIfNotBlank(this.Condition);
		block.AddIfHasElements(this.ClCompile.GetBlock());
		block.AddIfHasElements(this.Link.GetBlock());
		block.AddIfHasElements(this.Lib.GetBlock());
		block.AddIfHasElements(this.ResourceCompile.GetBlock());
		block.AddIfHasElements(this.PreLinkEvent.GetBlock());
		block.AddIfHasElements(this.PostBuildEvent.GetBlock());
		block.AddIfHasElements(this.CustomBuildStep.GetBlock());
		return block;
	}
}
