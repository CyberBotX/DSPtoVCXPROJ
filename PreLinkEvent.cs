using System.Xml.Linq;

namespace DSPtoVCXPROJ;

/// <summary>
/// The <see cref="PreLinkEvent" /> block contains the information for the Pre-Link Event under Build Events for a project.
/// </summary>
class PreLinkEvent : BuildEvent
{
	public override XElement GetBlock()
	{
		var block = base.GetBlock();
		block.Name = Program.NS + nameof(PreLinkEvent);
		return block;
	}
}
