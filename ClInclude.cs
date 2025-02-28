using System.Xml.Linq;

namespace DSPtoVCXPROJ;

/// <summary>
/// The <see cref="ClInclude" /> block is typically only used on C/C++ header files.
/// </summary>
class ClInclude(string? include = null) : None(include)
{
	public override XElement GetBlock()
	{
		var block = base.GetBlock();
		block.Name = Program.NS + nameof(ClInclude);
		return block;
	}
}
