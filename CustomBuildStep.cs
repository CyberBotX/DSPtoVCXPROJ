using System.Xml.Linq;

namespace DSPtoVCXPROJ;

/// <summary>
/// The <see cref="CustomBuildStep" /> block contains the information for the Custom Build Step for a project.
/// (NOTE: In VS 6.0, some files could also have custom build steps, but this is not the case in modern VS.)
/// </summary>
class CustomBuildStep
{
	// Order is based on how Visual Studio 2022 has them in its properties window
	public string? Command { get; set; }
	public string? Message { get; set; }
	public string? Outputs { get; set; }

	/// <summary>
	/// Gets the block as an <see cref="XElement" />.
	/// </summary>
	/// <returns>The block as an <see cref="XElement" />.</returns>
	public XElement GetBlock()
	{
		var block = Program.CreateElement(nameof(CustomBuildStep));
		block.AddIfNotBlank(this.Command);
		block.AddIfNotBlank(this.Message);
		block.AddIfNotBlank(this.Outputs);
		return block;
	}
}
