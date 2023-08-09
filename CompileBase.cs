using System.Xml.Linq;

namespace DSPtoVCXPROJ;

/// <summary>
/// A base class for both <see cref="ClCompile" /> and <see cref="ResourceCompile" />, as both contain an optional condition and create
/// their elements in the same way.
/// </summary>
abstract class CompileBase : None
{
	public string? Condition { get; }

	protected CompileBase() : base()
	{
	}

	protected CompileBase(string include, string condition) : base(include) => this.Condition = condition;

	public override XElement CreateElement(string name, object value)
	{
		var element = base.CreateElement(name, value);
		element.AddAttributeIfNotBlank(this.Condition);
		return element;
	}
}
