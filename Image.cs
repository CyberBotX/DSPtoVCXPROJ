using System.Xml.Linq;

namespace DSPtoVCXPROJ;

/// <summary>
/// The <see cref="Image" /> block is typically only used on Image files, such as .ico or .bmp files.
/// </summary>
class Image : None
{
	public Image(string? include = null) : base(include)
	{
	}

	public override XElement GetBlock()
	{
		var block = base.GetBlock();
		block.Name = Program.NS + nameof(Image);
		return block;
	}
}
