using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace DSPtoVCXPROJ;

static class Extensions
{
	/// <summary>
	/// Adds an <see cref="XElement" /> child to an existing <see cref="XElement" />, with the given string value and the name coming from
	/// the call to the method, but only if the value is not null or whitespace.
	/// </summary>
	/// <param name="element">The <see cref="XElement" /> to add the child to.</param>
	/// <param name="value">The value to add, if it isn't null or whitespace.</param>
	/// <param name="name">
	/// The name for the element, which comes from the expression for <paramref name="value" /> and should not need to be supplied.
	/// </param>
	public static void AddIfNotBlank(this XElement element, string? value, [CallerArgumentExpression(nameof(value))] string name = null!)
	{
		if (!string.IsNullOrWhiteSpace(value))
		{
			var nameSpan = name.AsSpan();
			int dot = nameSpan.LastIndexOf('.');
			if (dot != -1)
				nameSpan = nameSpan[(dot + 1)..]; // The Range is to remove things like 'this.'
			element.Add(Program.CreateElement($"{nameSpan}", value));
		}
	}

	/// <summary>
	/// Adds a <see cref="XAttribute" /> child to an existing <see cref="XElement" />, with the given string value and the name coming from
	/// the call to the method, but only if the value is not null or whitespace.
	/// </summary>
	/// <param name="element">The <see cref="XElement" /> to add the child to.</param>
	/// <param name="value">The value to add, if it isn't null or whitespace.</param>
	/// <param name="name">
	/// The name for the element, which comes from the expression for <paramref name="value" /> and should not need to be supplied.
	/// </param>
	public static void AddAttributeIfNotBlank(this XElement element, string? value,
		[CallerArgumentExpression(nameof(value))] string name = null!)
	{
		if (!string.IsNullOrWhiteSpace(value))
		{
			var nameSpan = name.AsSpan();
			int dot = nameSpan.LastIndexOf('.');
			if (dot != -1)
				nameSpan = nameSpan[(dot + 1)..]; // The Range is to remove things like 'this.'
			element.Add(new XAttribute($"{nameSpan}", value));
		}
	}

	/// <summary>
	/// Adds an <see cref="XElement" /> child to an existing <see cref="XElement" />, with the given boolean value and the name coming from
	/// the call to the method, but only if the value is not null.
	/// </summary>
	/// <param name="element">The <see cref="XElement" /> to add the child to.</param>
	/// <param name="value">The value to add, if it isn't null.</param>
	/// <param name="name">
	/// The name for the element, which comes from the expression for <paramref name="value" /> and should not need to be supplied.
	/// </param>
	public static void AddIfNotNull(this XElement element, bool? value, [CallerArgumentExpression(nameof(value))] string name = null!)
	{
		if (value is not null)
		{
			var nameSpan = name.AsSpan();
			int dot = nameSpan.LastIndexOf('.');
			if (dot != -1)
				nameSpan = nameSpan[(dot + 1)..]; // The Range is to remove things like 'this.'
			element.Add(Program.CreateElement($"{nameSpan}", value == true ? "true" : "false"));
		}
	}

	/// <summary>
	/// Adds an <see cref="XElement" /> block to an existing <see cref="XElement" />, but only if the block has elements.
	/// </summary>
	/// <param name="element">The <see cref="XElement" /> to add the block to.</param>
	/// <param name="block">The <see cref="XElement" /> with the block to add.</param>
	public static void AddIfHasElements(this XElement element, XElement block)
	{
		if (block.HasElements)
			element.Add(block);
	}
}
