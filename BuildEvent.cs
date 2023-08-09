using System.Xml.Linq;

namespace DSPtoVCXPROJ;

/// <summary>
/// A base class for both <see cref="PreLinkEvent" /> and <see cref="PostBuildEvent" />, as both contain a command and a message, and
/// create their elements in the same way.
/// </summary>
abstract class BuildEvent
{
	public string? Command { get; set; }
	public string? Message { get; set; }

	/// <summary>
	/// Gets the block as an <see cref="XElement" />.
	/// </summary>
	/// <returns>The block as an <see cref="XElement" />.</returns>
	public virtual XElement GetBlock()
	{
		var block = Program.CreateElement(nameof(BuildEvent));
		block.AddIfNotBlank(this.Command);
		block.AddIfNotBlank(this.Message);
		return block;
	}
}
