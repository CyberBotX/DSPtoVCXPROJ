using System.Xml.Linq;

namespace DSPtoVCXPROJ;

/// <summary>
/// The <see cref="PostBuildEvent" /> block contains the information for the Post-Build Event under Build Events for a project.
/// </summary>
class PostBuildEvent : BuildEvent
{
	public override XElement GetBlock()
	{
		var block = base.GetBlock();
		block.Name = Program.NS + nameof(PostBuildEvent);
		return block;
	}
}
