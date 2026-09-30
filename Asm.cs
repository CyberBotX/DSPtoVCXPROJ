using System.Xml.Linq;

namespace DSPtoVCXPROJ;

/// <summary>
/// Assembly source files (.asm). There is no built-in MSBuild item type for these unless the MASM build customization is imported, so they
/// are written as <see cref="None" /> items, but they can still carry the per-configuration ExcludedFromBuild flag from the .DSP file.
/// </summary>
class Asm(string include, string condition) : CompileBase(include, condition)
{
	public bool? ExcludedFromBuild { get; set; }

	public override XElement GetBlock()
	{
		var block = base.GetBlock();
		// The same object is used to create an entry in the filters file as well as the project file, so properties are only written in the
		// project file.
		if (this.Filter is null)
			this.AddIfNotNull(block, this.ExcludedFromBuild);
		return block;
	}
}
