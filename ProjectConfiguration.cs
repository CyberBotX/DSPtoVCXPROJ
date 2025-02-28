using System.Xml.Linq;

namespace DSPtoVCXPROJ;

/// <summary>
/// The <see cref="ProjectConfiguration" /> block contains a project configuration and platform, as it cannot be assumed that a project
/// only targets Win32 and only had Debug and Release configurations.
/// </summary>
class ProjectConfiguration(string configuration, string platform)
{
	public string Configuration { get; } = configuration;
	public string Platform { get; } = platform;
	public ConfigurationProperties Properties { get; } = new($"'$(Configuration)|$(Platform)'=='{configuration}|{platform}'");

	/// <summary>
	/// Gets the block as an <see cref="XElement" />.
	/// </summary>
	/// <returns>The block as an <see cref="XElement" />.</returns>
	public XElement GetBlock()
	{
		var block = Program.CreateElement(nameof(ProjectConfiguration),
			new XAttribute("Include", $"{this.Configuration}|{this.Platform}")
		);
		block.AddIfNotBlank(this.Configuration);
		block.AddIfNotBlank(this.Platform);
		return block;
	}
}
