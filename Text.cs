using System.Xml.Linq;

namespace DSPtoVCXPROJ;

/// <summary>
/// The <see cref="Text" /> block is typically only used on Text files, such as .txt files.
/// </summary>
class Text : None
{
	public Text(string? include = null) : base(include)
	{
	}

	public override XElement GetBlock()
	{
		var block = base.GetBlock();
		block.Name = Program.NS + nameof(Text);
		return block;
	}
}
