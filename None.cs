using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace DSPtoVCXPROJ;

/// <summary>
/// The base class for files, meant to contain the filename as well as a filter. Because it is also used as the base for project-wide
/// objects, the filename and filter are optional so they can be unused on project-wide objects. It is also meant for any files in the
/// project that don't have a specific type.
/// </summary>
class None
{
	public string? Include { get; }
	public Filter? Filter { get; set; }

	public None(string? include = null) => this.Include = include;

	/// <summary>
	/// Gets the block as an <see cref="XElement" />.
	/// </summary>
	/// <returns>The block as an <see cref="XElement" />.</returns>
	public virtual XElement GetBlock()
	{
		var block = Program.CreateElement(nameof(None));
		block.AddAttributeIfNotBlank(this.Include);
		if (this.Filter is not null)
			block.Add(this.Filter.GetBlock(false));
		return block;
	}

	/// <summary>
	/// Normally calls <see cref="Program.CreateElement" /> to create a generic <see cref="XElement "/> block, but can be overridden in
	/// order to do more than that.
	/// </summary>
	/// <param name="name">The name of the element.</param>
	/// <param name="elements">The child elements to add to the block.</param>
	/// <returns>The generic <see cref="XElement" /> block.</returns>
	public virtual XElement CreateElement(string name, object value) => Program.CreateElement(name, value);

	/// <summary>
	/// Adds an <see cref="XElement" /> child to an existing <see cref="XElement" />, with the given string value and the name coming from
	/// the call to the method, but only if the value is not null or whitespace.
	/// </summary>
	/// <param name="block">The <see cref="XElement" /> to add the child to.</param>
	/// <param name="value">The value to add, if it isn't null or whitespace.</param>
	/// <param name="name">
	/// The name for the element, which comes from the expression for <paramref name="value" /> and should not need to be supplied.
	/// </param>
	public void AddIfNotBlank(XElement block, string? value, [CallerArgumentExpression(nameof(value))] string name = null!)
	{
		// The range on name skips 'this.' in the expression for 'name'
		if (!string.IsNullOrWhiteSpace(value))
			block.Add(this.CreateElement(name[5..], value));
	}

	/// <summary>
	/// Adds an <see cref="XElement" /> child to an existing <see cref="XElement" />, with the given <see cref="HashSet{T}" /> of string
	/// values and the name coming from the call to the method, but only if the set is not empty. The values in the set will be joined by a
	/// separator before the element is added.
	/// </summary>
	/// <param name="block">The <see cref="XElement" /> to add the child to.</param>
	/// <param name="set">The set to add, if it isn't null.</param>
	/// <param name="separator">The separator to use between elements of the set.</param>
	/// <param name="name">
	/// The name for the element, which comes from the expression for <paramref name="set" /> and should not need to be supplied.
	/// </param>
	public void AddIfNotEmpty(XElement block, HashSet<string> set, char separator,
		[CallerArgumentExpression(nameof(set))] string name = null!)
	{
		// The range on name skips 'this.' in the expression for 'name'
		if (set.Count > 0)
			block.Add(this.CreateElement(name[5..], string.Join(separator, set.Append($"%({name[5..]})"))));
	}

	/// <summary>
	/// Adds an <see cref="XElement" /> child to an existing <see cref="XElement" />, with the given boolean value and the name coming from
	/// the call to the method, but only if the value is not null.
	/// </summary>
	/// <param name="block">The <see cref="XElement" /> to add the child to.</param>
	/// <param name="value">The value to add, if it isn't null.</param>
	/// <param name="name">
	/// The name for the element, which comes from the expression for <paramref name="value" /> and should not need to be supplied.
	/// </param>
	public void AddIfNotNull(XElement block, bool? value, [CallerArgumentExpression(nameof(value))] string name = null!)
	{
		// The range on name skips 'this.' in the expression for 'name'
		if (value is not null)
			block.Add(this.CreateElement(name[5..], value == true ? "true" : "false"));
	}
}
