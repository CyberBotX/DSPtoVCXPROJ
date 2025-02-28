using System.Xml.Linq;

namespace DSPtoVCXPROJ;

/// <summary>
/// The <see cref="Text" /> block is typically only used on Text files, such as .txt files.
/// </summary>
class Text(string? include = null) : None(include)
{
	public override XElement GetBlock()
	{
		var block = base.GetBlock();
		block.Name = Program.NS + nameof(Text);
		return block;
	}
}
