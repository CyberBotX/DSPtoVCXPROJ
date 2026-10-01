namespace DSPtoVCXPROJ;

/// <summary>
/// A wrapper around multiple <see cref="Asm" /> objects, to allow all of them to set the same property all at once.
/// </summary>
class MultipleAsm
{
	public List<Asm> Objects { get; set; } = [];

	public bool? ExcludedFromBuild
	{
		set
		{
			foreach (var obj in this.Objects)
				obj.ExcludedFromBuild = value;
		}
	}
}
