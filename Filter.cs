using System.Xml.Linq;

namespace DSPtoVCXPROJ;

/// <summary>
/// The <see cref="Filter" /> block contains the information for a filter to be applied to an object, so it can be placed within a specific
/// filter in the IDE. This encompasses both the general filters, which contain the extensions, as well as being used on the objects
/// themselves to say which filter they are contained within.
/// </summary>
class Filter(string include)
{
	public string Include { get; } = include;
	public string? Extensions { get; set; }

	/// <summary>
	/// Gets the block as an <see cref="XElement" />.
	/// </summary>
	/// <param name="IncludeAsAttrib">
	/// If <see langword="true" />, the block is generated for the primary ItemGroup of the filters file,
	/// if <see langword="false" />, the block is generated for an individual item.
	/// </param>
	/// <param name="elements">Additional child <see cref="XElement" />s for the block.</param>
	/// <returns>The block as an <see cref="XElement" />.</returns>
	public XElement GetBlock(bool IncludeAsAttrib, params object[] elements)
	{
		var block = Program.CreateElement(nameof(Filter), elements);
		if (IncludeAsAttrib)
		{
			block.AddAttributeIfNotBlank(this.Include);
			block.AddIfNotBlank(this.Extensions);
		}
		else
			block.Add(this.Include);
		return block;
	}
}
