using System.Xml.Linq;

namespace DSPtoVCXPROJ;

/// <summary>
/// The <see cref="Image" /> block is typically only used on Image files, such as .ico or .bmp files.
/// </summary>
class Image(string? include = null) : None(include)
{
	public override XElement GetBlock()
	{
		var block = base.GetBlock();
		block.Name = Program.NS + nameof(Image);
		return block;
	}
}
