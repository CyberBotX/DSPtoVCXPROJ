using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml.Linq;

namespace DSPtoVCXPROJ;

// The purpose of this program is to convert the .DSP project files used in Visual Studio 6.0 (and maybe also 5.0?) into .VCXPROJ project
// files for use in modern version of Visual Studio. This MIGHT work for .DSP project files from other versions of Visual Studio, but 6.0
// is primarily what was tested. (Testing is hard because finding .DSP project files in the wild these days is hard.)

// This will only support VS 2015 and later, because at the time of (re-)doing this, those are the only ones with readily available docs.
// Knowing which VS 6.0 options were and weren't available was only possible from installing VS 6.0 inside of a VM.

// The BSC32 sections are ignored, as modern versions of Visual Studio do not use it anymore.
// While it is possible it might not be correct, MTL sections are also ignored, if only because modern Visual Studio doesn't have a good
//   way to handle the MIDL arguments.

static class Program
{
	/// <summary>
	/// The XML namespace for the entirety of the MSBuild XML files.
	/// </summary>
	internal static readonly XNamespace NS = XNamespace.Get("http://schemas.microsoft.com/developer/msbuild/2003");
	/// <summary>
	/// The project's configuration type, set when # TARGTYPE is encountered.
	/// </summary>
	internal static string ConfigurationType = null!;

	/// <summary>
	/// The groups that are created by modern versions of Visual Studio for a new project, which differ from how Visual Studio 6.0 did them.
	/// </summary>
	static readonly Dictionary<string, (string Extensions, Guid GUID)> ExistingGroups = new()
	{
		["Source Files"] = (
			"cpp;c;cc;cxx;c++;cppm;ixx;def;odl;idl;hpj;bat;asm;asmx",
			new("{4FC737F1-C7A5-4376-A066-2A32D752A2FF}")
		),
		["Header Files"] = (
			"h;hh;hpp;hxx;h++;hm;inl;inc;ipp;xsd",
			new("{93995380-89BD-4b04-88EB-625FBE52EBFB}")
		),
		["Resource Files"] = (
			"rc;ico;cur;bmp;dlg;rc2;rct;bin;rgs;gif;jpg;jpeg;jpe;resx;tiff;tif;png;wav;mfcribbon-ms",
			new("{67DA6AB6-F800-4c08-8B7A-83BB121AAD01}")
		)
	};

	/// <summary>
	/// A collection of <see cref="ProjectConfiguration" /> elements, using <see cref="KeyedCollection{TKey, TItem}" /> to allow them to
	/// be accessed by key while still being stored like a list.
	/// </summary>
	internal class ProjectConfigurations : KeyedCollection<string, ProjectConfiguration>
	{
		protected override string GetKeyForItem(ProjectConfiguration item) => $"{item.Platform} {item.Configuration}";
	}

	/// <summary>
	/// Throws an <see cref="ArgumentException" /> with the given data for an invalid value.
	/// </summary>
	/// <param name="name">The name (either an option name or a program name) that had the invalid value.</param>
	/// <param name="label">The label of what was invalid (usually something like "argument" or "flag").</param>
	/// <param name="value">The value that was invalid.</param>
	/// <param name="lineNumber">The line number where the invalid value was found.</param>
	/// <param name="valueName">The optional name of the value to append after <paramref name="name" />.</param>
	/// <returns>
	/// This method does not actually return, but the return value is needed to prevent the compiler from thinking that there might be code
	/// after this call, despite the <see cref="DoesNotReturnAttribute" />.
	/// </returns>
	[DoesNotReturn]
	static Exception ThrowInvalid(ReadOnlySpan<char> name, ReadOnlySpan<char> label, ReadOnlySpan<char> value, int lineNumber,
		string? valueName = null) =>
		throw new ArgumentException($"Invalid {name}{(valueName is null ? "" : $" {valueName}")} {label} [{value}] - Line {lineNumber}");
	/// <summary>
	/// Throws an <see cref="ArgumentException" /> with the given data for an invalid value.
	/// </summary>
	/// <typeparam name="T">The type is only needed to allow this to be called in places that expect a return value.</typeparam>
	/// <param name="name">The name (either an option name or a program name) that had the invalid value.</param>
	/// <param name="label">The label of what was invalid (usually something like "argument" or "flag").</param>
	/// <param name="value">The value that was invalid.</param>
	/// <param name="lineNumber">The line number where the invalid value was found.</param>
	/// <param name="valueName">The optional name of the value to append after <paramref name="name" />.</param>
	/// <returns>
	/// This method does not actually return, but the return value is needed to prevent the compiler from thinking that there might be code
	/// after this call, despite the <see cref="DoesNotReturnAttribute" />.
	/// </returns>
	[DoesNotReturn]
	static T ThrowInvalid<T>(ReadOnlySpan<char> name, ReadOnlySpan<char> label, ReadOnlySpan<char> value, int lineNumber,
		string? valueName = null) =>
		throw new ArgumentException($"Invalid {name}{(valueName is null ? "" : $" {valueName}")} {label} [{value}] - Line {lineNumber}");

	/// <summary>
	/// Throws an <see cref="ArgumentException" /> with the given data for an invalid argument.
	/// </summary>
	/// <param name="name">The name (either an option name or a program name) that had the invalid argument.</param>
	/// <param name="argument">The argument that was invalid.</param>
	/// <param name="lineNumber">The line number where the invalid argument was found.</param>
	/// <returns>
	/// This method does not actually return, but the return value is needed to prevent the compiler from thinking that there might be code
	/// after this call, despite the <see cref="DoesNotReturnAttribute" />.
	/// </returns>
	[DoesNotReturn]
	static Exception ThrowInvalidArgument(ReadOnlySpan<char> name, ReadOnlySpan<char> argument, int lineNumber) =>
		Program.ThrowInvalid(name, "argument", argument, lineNumber);
	/// <summary>
	/// Throws an <see cref="ArgumentException" /> with the given data for an invalid argument.
	/// </summary>
	/// <typeparam name="T">The type is only needed to allow this to be called in places that expect a return value.</typeparam>
	/// <param name="name">The name (either an option name or a program name) that had the invalid argument.</param>
	/// <param name="argument">The argument that was invalid.</param>
	/// <param name="lineNumber">The line number where the invalid argument was found.</param>
	/// <returns>
	/// This method does not actually return, but the return value is needed to prevent the compiler from thinking that there might be code
	/// after this call, despite the <see cref="DoesNotReturnAttribute" />.
	/// </returns>
	[DoesNotReturn]
	static T ThrowInvalidArgument<T>(ReadOnlySpan<char> name, ReadOnlySpan<char> argument, int lineNumber) =>
		Program.ThrowInvalid<T>(name, "argument", argument, lineNumber);

	/// <summary>
	/// Throws an <see cref="ArgumentException" /> with the given data for an invalid flag.
	/// </summary>
	/// <param name="name">The name (either an option name or a program name) that had the invalid flag.</param>
	/// <param name="flag">The flag that was invalid.</param>
	/// <param name="lineNumber">The line number where the invalid flag was found.</param>
	/// <param name="flagName">The optional name of the flag to append after <paramref name="name" />.</param>
	/// <returns>
	/// This method does not actually return, but the return value is needed to prevent the compiler from thinking that there might be code
	/// after this call, despite the <see cref="DoesNotReturnAttribute" />.
	/// </returns>
	[DoesNotReturn]
	static Exception ThrowInvalidFlag(ReadOnlySpan<char> name, ReadOnlySpan<char> flag, int lineNumber, string? flagName = null) =>
		Program.ThrowInvalid(name, "flag", flag, lineNumber, flagName);
	/// <summary>
	/// Throws an <see cref="ArgumentException" /> with the given data for an invalid flag.
	/// </summary>
	/// <typeparam name="T">The type is only needed to allow this to be called in places that expect a return value.</typeparam>
	/// <param name="name">The name (either an option name or a program name) that had the invalid flag.</param>
	/// <param name="flag">The flag that was invalid.</param>
	/// <param name="lineNumber">The line number where the invalid flag was found.</param>
	/// <param name="flagName">The optional name of the flag to append after <paramref name="name" />.</param>
	/// <returns>
	/// This method does not actually return, but the return value is needed to prevent the compiler from thinking that there might be code
	/// after this call, despite the <see cref="DoesNotReturnAttribute" />.
	/// </returns>
	[DoesNotReturn]
	static T ThrowInvalidFlag<T>(ReadOnlySpan<char> name, ReadOnlySpan<char> flag, int lineNumber, string? flagName = null) =>
		Program.ThrowInvalid<T>(name, "flag", flag, lineNumber, flagName);

	/// <summary>
	/// Gathers up files of the given type only and adds them to an ItemGroup in both the main project file and the filters file.
	/// </summary>
	/// <typeparam name="T">The specific type that needs to be added to the files.</typeparam>
	/// <param name="vcxprojRootXML">The XML containing the main project file.</param>
	/// <param name="filtersRootXML">The XML containing the filters file.</param>
	/// <param name="files">The files to be filtered and added.</param>
	static void GetFiles<T>(XElement vcxprojRootXML, XElement filtersRootXML, List<SourceFile> files) where T : None
	{
		// Create the ItemGroup blocks for both files.
		var filtersItemGroup = Program.CreateItemGroupBlock(null);
		var vcxprojItemGroup = Program.CreateItemGroupBlock(null);
		// I can't do "f.Debug is T" because when T is None, it'll catch everything, and I don't want that.
		foreach (var file in files.Where(static f => f.Configurations.Values.First().GetType() == typeof(T)))
		{
			var values = file.Configurations.Values;
			var firstValue = values.First();
			// Create the filter block first, if there is a filter, and add it to the filters file.
			if (firstValue.Filter is not null)
				filtersItemGroup.Add(firstValue.GetBlock());
			// Remove all the filters so we can get the blocks for the project file.
			foreach (var config in values)
				config.Filter = null;
			// Create the project block from the first configuration, then add the extra elements from all the other configurations.
			var vcxprojItemGroupBlock = firstValue.GetBlock();
			foreach (var config in values.Skip(1))
				foreach (var elem in config.GetBlock().Elements())
				{
					elem.Remove();
					vcxprojItemGroupBlock.Add(elem);
				}
			// Add the project block to the project file.
			vcxprojItemGroup.Add(vcxprojItemGroupBlock);
		}
		// Only add the ItemGroup blocks if they had elements.
		if (filtersItemGroup.HasElements)
			filtersRootXML.Add(filtersItemGroup);
		if (vcxprojItemGroup.HasElements)
			vcxprojRootXML.Add(vcxprojItemGroup);
	}

	/// <summary>
	/// Creates an ItemGroup block, with optional label.
	/// </summary>
	/// <param name="Label">The optional value for the Label attribute.</param>
	/// <param name="elements">The child elements to add to the block.</param>
	/// <returns>The ItemGroup <see cref="XElement" /> block.</returns>
	static XElement CreateItemGroupBlock(string? Label, params object[] elements)
	{
		var block = Program.CreateElement("ItemGroup", elements);
		block.AddAttributeIfNotBlank(Label);
		return block;
	}

	/// <summary>
	/// Creates a PropertyGroup block, with optional condition and label.
	/// </summary>
	/// <param name="Condition">The optional value for the Condtional attribute.</param>
	/// <param name="Label">The optional value for the Label attribute.</param>
	/// <param name="elements">The child elements to add to the block.</param>
	/// <returns>The PropertyGroup <see cref="XElement" /> block.</returns>
	internal static XElement CreatePropertyGroupBlock(string? Condition, string? Label, params object[] elements)
	{
		var block = Program.CreateElement("PropertyGroup", elements);
		block.AddAttributeIfNotBlank(Condition);
		block.AddAttributeIfNotBlank(Label);
		return block;
	}

	/// <summary>
	/// Creates an Import block for a given project, with optional conditional and label.
	/// </summary>
	/// <param name="Project">The value for the Project attribute.</param>
	/// <param name="Condition">The optional value for the Conditional attribute.</param>
	/// <param name="Label">The optional value for the Label attribute.</param>
	/// <returns>The Import <see cref="XElement" /> block.</returns>
	static XElement CreateImportBlock(string Project, string? Condition, string? Label)
	{
		var block = Program.CreateElement("Import");
		block.AddAttributeIfNotBlank(Project);
		block.AddAttributeIfNotBlank(Condition);
		block.AddAttributeIfNotBlank(Label);
		return block;
	}

	/// <summary>
	/// Creates an ImportGroup block, with condition and label.
	/// </summary>
	/// <param name="Condition">The value for the Condition attribute.</param>
	/// <param name="Label">The value for the Label attribute.</param>
	/// <param name="elements">The child elements to add to the block.</param>
	/// <returns>The ImportGroup <see cref="XElement" /> block.</returns>
	static XElement CreateImportGroupBlock(string Condition, string Label, params object[] elements)
	{
		var block = Program.CreateElement("ImportGroup", elements);
		block.AddAttributeIfNotBlank(Condition);
		block.AddAttributeIfNotBlank(Label);
		return block;
	}

	/// <summary>
	/// Creates a generic <see cref="XElement" /> block with the MSBuild namespace attached to it.
	/// </summary>
	/// <param name="name">The name of the element.</param>
	/// <param name="elements">The child elements to add to the block.</param>
	/// <returns>The generic <see cref="XElement" /> block.</returns>
	internal static XElement CreateElement(string name, params object[] elements) => new(Program.NS + name, elements);

	static string rootNamespace = null!;
	static readonly ProjectConfigurations projectConfigurations = [];
	static ProjectConfiguration? currentConfig = null;
	static readonly List<Filter> filters = [];
	static Filter? currentFilter = null;
	static readonly MultipleClCompile multipleClCompile = new();
	static readonly MultipleResourceCompile multipleResourceCompile = new();
	static readonly List<SourceFile> files = [];
	static SourceFile? currentFile = null;

	/// <summary>
	/// Process a line containing a property.
	/// </summary>
	/// <param name="props">The contents of the line.</param>
	/// <param name="lineNumber">The line number of the line.</param>
	static void ProcessProps(ReadOnlySpan<char> props, int lineNumber)
	{
		int spaces = props.Count(' ');
		Span<Range> ranges = stackalloc Range[spaces + 1];
		_ = props.Split(ranges, ' ', StringSplitOptions.RemoveEmptyEntries);
		var props3 = props[ranges[3]];
		switch (props[ranges[2]])
		{
			case "Use_MFC":
				switch (int.Parse(props3, CultureInfo.InvariantCulture))
				{
					case 0:
						break; // Ignored because this is the default
					case 1 or 5:
						Program.currentConfig!.Properties.UseOfMfc = "Static";
						break;
					case 2 or 6:
						Program.currentConfig!.Properties.UseOfMfc = "Dynamic";
						break;
					default:
						throw Program.ThrowInvalidArgument("Use_MFC", props3, lineNumber);
				}
				break;
			case "Use_Debug_Libraries":
				switch (int.Parse(props3, CultureInfo.InvariantCulture))
				{
					case 0:
						break; // Ignored because this is the default
					case 1:
						Program.currentConfig!.Properties.UseDebugLibraries = true;
						break;
					default:
						throw Program.ThrowInvalidArgument("Use_Debug_Libraries", props3, lineNumber);
				}
				break;
			case "Output_Dir":
				Program.currentConfig!.Properties.OutDir = $"{props3.Trim('"')}";
				break;
			case "Intermediate_Dir":
				Program.currentConfig!.Properties.IntDir = $"{props3.Trim('"')}";
				break;
			case "Default_Filter":
				if (Program.currentFilter is not null)
				{
					var filter = props3.Trim('"');
					if (filter.Length != 0)
						Program.currentFilter.Extensions = $"{filter}";
				}
				break;
			case "Exclude_From_Build":
				Program.multipleClCompile.ExcludedFromBuild = int.Parse(props3, CultureInfo.InvariantCulture) switch
				{
					0 => false,
					1 => true,
					_ => Program.ThrowInvalidArgument<bool>("Exclude_From_Build", props3, lineNumber)
				};
				break;
		}
	}

	/// <summary>
	/// Process a line for the compiler.
	/// </summary>
	/// <param name="cppFlags">The contents of the line.</param>
	/// <param name="lineNumber">The line number of the line.</param>
	static void ProcessCPP(ReadOnlySpan<char> cppFlags, int lineNumber)
	{
		bool hadNoLogo = false;
		cppFlags = cppFlags[(cppFlags.Contains("BASE", StringComparison.InvariantCulture) ? 15 : 10)..];
		int spaces = cppFlags.Count(' ');
		Span<Range> ranges = stackalloc Range[spaces + 1];
		int numCppFlags = cppFlags.Split(ranges, ' ', StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < numCppFlags; ++i)
		{
			var cppFlag = cppFlags[ranges[i]];
			if (cppFlag[0] is '/' or '-')
			{
				cppFlag = cppFlag[1..];
				switch (cppFlag)
				{
					case "C":
						Program.multipleClCompile.PreprocessKeepComments = true;
						break;
					case var _ when cppFlag.StartsWith("D", StringComparison.InvariantCulture):
					{
						// Allowed to have a space between the flag and the argument
						var define = (cppFlag.Length > 1 ? cppFlag[1..] : cppFlags[ranges[++i]]).Trim('"');
						switch (define)
						{
							case "_MBCS":
								Program.currentConfig!.Properties.CharacterSet = "MultiByte";
								break;
							case "_UNICODE":
								Program.currentConfig!.Properties.CharacterSet = "Unicode";
								break;
							default:
								Program.multipleClCompile.PreprocessorDefinitionsAdd($"{define}");
								if (define is "_AFXEXT")
									Program.multipleClCompile.PreprocessorDefinitionsAdd("_WINDLL");
								break;
						}
						break;
					}
					case { Length: > 2 } when cppFlag.StartsWith("EH", StringComparison.InvariantCulture):
					{
						var ehFlag = cppFlag[2..];
						// Ignoring if /EHsc was being set, as this is the default
						if (ehFlag is not "sc")
							Program.multipleClCompile.ExceptionHandling = ehFlag switch
							{
								"a" => "Async",
								"s" => "SyncCThrow",
								_ => Program.ThrowInvalidFlag<string>("cl.exe", ehFlag, lineNumber, "/EH")
							};
						break;
					}
					case "EP":
						Program.multipleClCompile.PreprocessSuppressLineNumbers = true;
						break;
					case var _ when cppFlag.StartsWith("F", StringComparison.InvariantCulture):
					{
						var fFlag = cppFlag.Length > 1 ? cppFlag[1..] : new();
						switch (fFlag)
						{
							case "A":
								Program.multipleClCompile.AssemblerOutput = "AssemblyCode";
								break;
							case "Ac":
								Program.multipleClCompile.AssemblerOutput = "AssemblyAndMachineCode";
								break;
							case "Acs":
								Program.multipleClCompile.AssemblerOutput = "All";
								break;
							case "As":
								Program.multipleClCompile.AssemblerOutput = "AssemblyAndSourceCode";
								break;
							case var _ when fFlag.StartsWith("a", StringComparison.InvariantCulture):
							{
								// Unsure if this will ever have a space after it or not
								var location = (fFlag.Length > 1 ? fFlag[1..] : cppFlags[ranges[++i]]).Trim('"');
								// Ignoring if location was $(IntDir), as this is the default
								if (location is not "$(IntDir)")
									Program.multipleClCompile.AssemblerListingLocation = $"{location}";
								break;
							}
							case { Length: > 1 } when fFlag.StartsWith("d", StringComparison.InvariantCulture):
							{
								// Not allowed to have a space between the flag and the argument
								var filename = fFlag[1..].Trim('"');
								// Ignoring if filename was $(IntDir)vc$(PlatformToolsetVersion).pdb, as this is the default
								if (filename is not "$(IntDir)vc$(PlatformToolsetVersion).pdb")
									Program.multipleClCompile.ProgramDataBaseFileName = $"{filename}";
								break;
							}
							case var _ when fFlag.StartsWith("I", StringComparison.InvariantCulture):
								// Allowed to have a space between the flag and the argument
								Program.multipleClCompile.
									ForcedIncludeFilesAdd($"{(fFlag.Length > 1 ? fFlag[2..] : cppFlags[ranges[++i]]).Trim('"')}");
								break;
							case { Length: > 1 } when fFlag.StartsWith("o", StringComparison.InvariantCulture):
							{
								// Not allowed to have a space between the flag and the argument
								var filename = fFlag[1..].Trim('"');
								// Ignoring if filename was $(IntDir), as this is the default
								if (filename is not "$(IntDir)")
									Program.multipleClCompile.ObjectFileName = $"{filename}";
								break;
							}
							case { Length: > 1 } when fFlag.StartsWith("p", StringComparison.InvariantCulture):
							{
								// Not allowed to have a space between the flag and the argument
								var filename = fFlag[1..].Trim('"');
								// Ignoring if filename was $(IntDir)$(TargetName).pch, as this is the default
								if (filename is not "$(IntDir)$(TargetName).pch")
									Program.multipleClCompile.PrecompiledHeaderOutputFile = $"{filename}";
								break;
							}
							case var _ when fFlag.StartsWith("R", StringComparison.InvariantCulture):
							case var _ when fFlag.StartsWith("r", StringComparison.InvariantCulture):
								// 'r' is deprecated but does nearly the same thing as 'R'
								Program.multipleClCompile.BrowseInformation = true;
								// Not allowed to have a space between the flag and the argument
								if (fFlag.Length > 1)
								{
									var filename = fFlag[1..].Trim('"');
									// Ignoring if filename was $(IntDir), as this is the default
									if (filename is not "$(IntDir)")
										Program.multipleClCompile.BrowseInformationFile = $"{filename}";
								}
								break;
							case { Length: > 1 } when fFlag.StartsWith("e", StringComparison.InvariantCulture):
							case { Length: > 1 } when fFlag.StartsWith("m", StringComparison.InvariantCulture):
							case { Length: 0 }: // Handles /F <num>
							case { Length: > 0 } when int.TryParse(fFlag.Trim('"'), out _): // Handles /F<num>
								// For /F, allowed to have a space between the flag and the argument (but we 'trim' that space away)
								if (cppFlag.Length == 0)
									Program.multipleClCompile.AdditionalOptionsAdd($"/F{cppFlags[ranges[++i]].Trim('"')}");
								else
									Program.multipleClCompile.AdditionalOptionsAdd($"{cppFlag}");
								break;
							case "D":
								break; // Ignored because it no longer exists in VS 2015 and later
							default:
								throw Program.ThrowInvalidFlag("cl.exe", fFlag, lineNumber, "/F");
						}
						break;
					}
					case { Length: > 1 } when cppFlag.StartsWith("G", StringComparison.InvariantCulture):
						switch (cppFlag[1..])
						{
							case "d":
								break; // Ignored as it is the default
							case "F" or "f":
								// Technically "f" no longer exists in VS 2015 and later, but probably good to handle here it too...
								Program.multipleClCompile.StringPooling = true;
								break;
							case "R":
								Program.multipleClCompile.RuntimeTypeInfo = true;
								break;
							case "R-":
								Program.multipleClCompile.RuntimeTypeInfo = false;
								break;
							case "r":
								Program.multipleClCompile.CallingConvention = "FastCall";
								break;
							case "T":
								Program.multipleClCompile.EnableFiberSafeOptimizations = true;
								break;
							case "X-":
								Program.multipleClCompile.ExceptionHandling = "false";
								break;
							case "y":
								Program.multipleClCompile.FunctionLevelLinking = true;
								break;
							case "Z": // Equivalent of /RTCs
								Program.multipleClCompile.BasicRuntimeChecks = "StackFrameRuntimeCheck";
								break;
							case "z":
								Program.multipleClCompile.CallingConvention = "StdCall";
								break;
							case "A" or "h":
							case { Length: > 2 } when cppFlag[1] == 's' && int.TryParse(cppFlag[2..], out _):
								// Not allowed to have a space between the flag and the argument for /Gs
								Program.multipleClCompile.AdditionalOptionsAdd($"{cppFlag}");
								break;
							case "X":
								break; // Ignored because this would be the same as setting /EHsc and this is the default
							case "e" or "m" or "m-":
								break; // Ignored because they are deprecated
							case "3" or "4" or "5" or "6" or "B" or "D" or "i" or "i-":
								break; // Ignored because they no longer exist in VS 2015 and later
							default:
								throw Program.ThrowInvalidFlag("cl.exe", cppFlag[1..], lineNumber, "/G");
						}
						break;
					case var _ when cppFlag.StartsWith("I", StringComparison.InvariantCulture):
						// Allowed to have a space between the flag and the argument
						Program.multipleClCompile.
							AdditionalIncludeDirectoriesAdd($"{(cppFlag.Length > 1 ? cppFlag[1..] : cppFlags[ranges[++i]]).Trim('"')}");
						break;
					case { Length: > 1 } when cppFlag.StartsWith("M", StringComparison.InvariantCulture):
						switch (cppFlag[1..])
						{
							case "D":
								// Ignoring if UseDebugLibraries is set to false (or not set at all), as this is the default
								if (Program.currentConfig!.Properties.UseDebugLibraries == true)
									Program.multipleClCompile.RuntimeLibrary = "MultiThreadedDLL";
								break;
							case "Dd":
								// Ignoring if UseDebugLibraries is set to true, as this is the default
								if (Program.currentConfig!.Properties.UseDebugLibraries != true)
									Program.multipleClCompile.RuntimeLibrary = "MultiThreadedDebugDLL";
								break;
							case "T":
								Program.multipleClCompile.RuntimeLibrary = "MultiThreaded";
								break;
							case "Td":
								Program.multipleClCompile.RuntimeLibrary = "MultiThreadedDebug";
								break;
							case "L" or "Ld":
								break; // Ignored because they no longer exist in VS 2015 and later
							default:
								throw Program.ThrowInvalidFlag("cl.exe", cppFlag[1..], lineNumber, "/M");
						}
						break;
					case "nologo":
						hadNoLogo = true;
						break;
					case { Length: > 1 } when cppFlag.StartsWith("O", StringComparison.InvariantCulture):
						switch (cppFlag[1..])
						{
							case "1":
								Program.multipleClCompile.Optimization = "MinSpace";
								break;
							case "2":
								// Ignoring if UseDebugLibraries is set to false (or not set at all), as this is the default
								if (Program.currentConfig!.Properties.UseDebugLibraries == true)
									Program.multipleClCompile.Optimization = "MaxSpeed";
								break;
							case "b0":
								Program.multipleClCompile.InlineFunctionExpansion = "Disabled";
								break;
							case "b1":
								Program.multipleClCompile.InlineFunctionExpansion = "OnlyExplicitInline";
								break;
							case "b2":
								Program.multipleClCompile.InlineFunctionExpansion = "AnySuitable";
								break;
							case "d":
								// Ignoring if UseDebugLibraries is set to true, as this is the default
								if (Program.currentConfig!.Properties.UseDebugLibraries != true)
									Program.multipleClCompile.Optimization = "Disabled";
								break;
							case "i":
								Program.multipleClCompile.IntrinsicFunctions = true;
								break;
							case "s":
								Program.multipleClCompile.FavorSizeOrSpeed = "Size";
								break;
							case "t":
								Program.multipleClCompile.FavorSizeOrSpeed = "Speed";
								break;
							case "x":
								Program.multipleClCompile.Optimization = "Full";
								break;
							case "y":
								Program.multipleClCompile.OmitFramePointers = true;
								break;
							case "y-":
								// Ignoring if Platform is Win32, as this is the default
								if (Program.currentConfig!.Platform != "Win32")
									Program.multipleClCompile.OmitFramePointers = false;
								break;
							case "g":
								break; // Ignored because it is deprecated
							case "a" or "p" or "p-" or "w":
								break; // Ignored because they no longer exist in VS 2015 and later
							default:
								throw Program.ThrowInvalidFlag("cl.exe", cppFlag[1..], lineNumber, "/O");
						}
						break;
					case "P":
						Program.multipleClCompile.PreprocessToFile = true;
						break;
					case { Length: 2 } when cppFlag.StartsWith("T", StringComparison.InvariantCulture):
						Program.multipleClCompile.CompileAs = cppFlag[1] switch
						{
							// NOTE: c or p SHOULD have a filename after them, but if they are encountered here,
							// then most likely the capitalized version was actually intended; normally I'd consider
							// this an error, as compiling a file as C or C++ is set individually on the file in the IDE
							'C' or 'c' => "CompileAsC",
							'P' or 'p' => "CompileAsCpp",
							_ => Program.ThrowInvalidFlag<string>("cl.exe", cppFlag[1..], lineNumber, "/T")
						};
						break;
					case var _ when cppFlag.StartsWith("U", StringComparison.InvariantCulture):
						// Allowed to have a space between the flag and the argument
						Program.multipleClCompile.
							UndefinePreprocessorDefinitionsAdd($"{(cppFlag.Length > 1 ? cppFlag[1..] : cppFlags[ranges[++i]]).Trim('"')}");
						break;
					case "u":
						Program.multipleClCompile.UndefineAllPreprocessorDefinitions = true;
						break;
					case var _ when cppFlag.StartsWith("W", StringComparison.InvariantCulture):
					{
						// Allowed to have a space between the flag and the argument
						var level = cppFlag.Length > 1 ? cppFlag[1..] : cppFlags[ranges[++i]];
						if (level.Length == 1 && char.IsNumber(level[0]))
						{
							// Ignoring /W1, as this is the default
							if (level[0] != '1')
								Program.multipleClCompile.WarningLevel = level[0] == '0' ? "TurnOffAllWarnings" : $"Level{level}";
						}
						else
							Program.multipleClCompile.TreatWarningAsError = (level.Length == 1 && level[0] == 'X') ||
								Program.ThrowInvalidFlag<bool>("cl.exe", level, lineNumber, "/W");
						break;
					}
					case "w":
						// Special case, it is the same as /W0
						Program.multipleClCompile.WarningLevel = "TurnOffAllWarnings";
						break;
					case "X":
						Program.multipleClCompile.IgnoreStandardIncludePath = true;
						break;
					case { Length: > 1 } when cppFlag.StartsWith("Y", StringComparison.InvariantCulture):
						switch (cppFlag[1])
						{
							case 'c':
								Program.multipleClCompile.PrecompiledHeader = "Create";
								break;
							case 'u':
								Program.multipleClCompile.PrecompiledHeader = "Use";
								break;
							case 'd':
								break; // Ignored because it is deprecated
							case 'X':
								break; // Ignored because it no longer exists in VS 2015 and later
							default:
								throw Program.ThrowInvalidFlag("cl.exe", cppFlag[2..], lineNumber, "/Y");
						}
						// Unsure if this will ever have a space after it or not (not handling that case because of it)
						if (cppFlag.Length > 2)
						{
							var filename = cppFlag[2..].Trim('"');
							// Ignoring if the filename was stdafx.h, as this is the default
							if (filename is not "stdafx.h")
								Program.multipleClCompile.PrecompiledHeaderFile = $"{filename}";
						}
						break;
					case { Length: > 1 } when cppFlag.StartsWith("Z", StringComparison.InvariantCulture):
						switch (cppFlag[1])
						{
							case '7':
								Program.multipleClCompile.DebugInformationFormat = "OldStyle";
								break;
							case 'a':
								Program.multipleClCompile.DisableLanguageExtensions = true;
								break;
							case 'I':
								// Ignoring if UseDebugLibraries is true, as this is the default
								if (Program.currentConfig!.Properties.UseDebugLibraries != true)
									Program.multipleClCompile.DebugInformationFormat = "EditAndContinue";
								break;
							case 'i':
								// Ignoring if UseDebugLibraries is false (or not set at all), as this is the default
								if (Program.currentConfig!.Properties.UseDebugLibraries == true)
									Program.multipleClCompile.DebugInformationFormat = "ProgramDatabase";
								break;
							case 'l':
								Program.multipleClCompile.OmitDefaultLibName = true;
								break;
							case 'p':
								switch (cppFlag)
								{
									case { Length: 2 }:
										break; // Ignored because it is the default
									case { Length: > 2 } when int.TryParse(cppFlag[2..], out int bytes) && bytes is 1 or 2 or 4 or 8 or 16:
										Program.multipleClCompile.StructMemberAlignment = $"{bytes}byte{(bytes == 1 ? "" : "s")}";
										break;
									default:
										throw Program.ThrowInvalidFlag("cl.exe", cppFlag[2..], lineNumber, "/Zp");
								}
								break;
							case 'm' when cppFlag.Length > 2 && int.TryParse(cppFlag[2..], out _):
							case 's': // This probably won't ever be set in the IDE
									  // Unsure if /Zm will ever have a space after it or not (not handling that case because of it)
								Program.multipleClCompile.AdditionalOptionsAdd($"{cppFlag}");
								break;
							case 'e':
								break; // Ignored because it is deprecated
							case 'd' or 'g' or 'n':
								break; // Ignored because they no longer exist in VS 2015 and later
							default:
								throw Program.ThrowInvalidFlag("cl.exe", cppFlag[1..], lineNumber, "/Z");
						}
						break;
					case "E" or "J" or "LD" or "LDd" or "link" or "vd0" or "vd1" or "vmb" or "vmg" or "vmm" or "vms" or "vmv":
						Program.multipleClCompile.AdditionalOptionsAdd($"{cppFlag}");
						if (cppFlag is "link")
							Program.multipleClCompile.AdditionalOptionsAdd($"{cppFlags[ranges[++i]]}");
						break;
					case "c":
						break; // Ignoring as there is really no reason this would ever be in an IDE project file
					case var _ when cppFlag.StartsWith("H", StringComparison.InvariantCulture):
					case var _ when cppFlag.StartsWith("V", StringComparison.InvariantCulture):
						// Ignoring because they are deprecated
						if (cppFlag is "H" or "V")
							++i; // Skips the next flag, as it is the argument for /H or /V
						break;
					case "noBool" or "QI0f" or "QI0f-" or "QIfdiv" or "QIfdiv-" or "Qlf":
						break; // Ignoring because they no longer exist in VS 2015 and later
					default:
						throw Program.ThrowInvalidFlag("cl.exe", cppFlag, lineNumber);
				}
			}
			else
				throw Program.ThrowInvalidArgument("cl.exe", cppFlag, lineNumber);
		}
		// This is being done like this because SuppressStartupBanner defaults to true
		if (Program.currentFile is null && !hadNoLogo)
			Program.multipleClCompile.SuppressStartupBanner = false;
	}

	/// <summary>
	/// Process a line for the linker.
	/// </summary>
	/// <param name="line">The contents of the line.</param>
	/// <param name="lineNumber">The line number of the line.</param>
	static void ProcessLink(ReadOnlySpan<char> line, int lineNumber)
	{
		bool hadNoLogo = false;
		line = line[(line.Contains("BASE", StringComparison.InvariantCulture) ? 18 : 13)..];
		int spaces = line.Count(' ');
		Span<Range> ranges = stackalloc Range[spaces + 1];
		int numRanges = line.Split(ranges, ' ', StringSplitOptions.RemoveEmptyEntries);
		foreach (var range in ranges[..numRanges])
		{
			var flag = line[range];
			var linkFlag = flag;
			if (linkFlag[0] is '/' or '-')
			{
				linkFlag = linkFlag[1..];
				switch (linkFlag)
				{
					case { Length: > 6 } when linkFlag.StartsWith("align:", StringComparison.InvariantCultureIgnoreCase):
						Program.currentConfig!.Properties.Link.SectionAlignment = $"{linkFlag[6..].Trim('"')}";
						break;
					case { Length: > 5 } when linkFlag.StartsWith("base:", StringComparison.InvariantCultureIgnoreCase):
						Program.currentConfig!.Properties.Link.BaseAddress = $"{linkFlag[5..].Trim('"')}";
						break;
					case var _ when linkFlag.Equals("debug", StringComparison.InvariantCultureIgnoreCase):
						Program.currentConfig!.Properties.Link.GenerateDebugInformation = true;
						break;
					case { Length: > 4 } when linkFlag.StartsWith("def:", StringComparison.InvariantCultureIgnoreCase):
						Program.currentConfig!.Properties.Link.ModuleDefinitionFile = $"{linkFlag[4..].Trim('"')}";
						break;
					case { Length: > 6 } when linkFlag.StartsWith("delay:", StringComparison.InvariantCultureIgnoreCase):
					{
						var delayFlag = linkFlag[6..];
						switch (delayFlag)
						{
							case var _ when delayFlag.Equals("nobind", StringComparison.InvariantCultureIgnoreCase):
								Program.currentConfig!.Properties.Link.SupportNobindOfDelayLoadedDLL = true;
								break;
							case var _ when delayFlag.Equals("unload", StringComparison.InvariantCultureIgnoreCase):
								Program.currentConfig!.Properties.Link.SupportUnloadOfDelayLoadedDLL = true;
								break;
							default:
								throw Program.ThrowInvalidFlag("link.exe", delayFlag, lineNumber, "/delay:");
						}
						break;
					}
					case { Length: > 10 } when linkFlag.StartsWith("delayload:", StringComparison.InvariantCultureIgnoreCase):
						_ = Program.currentConfig!.Properties.Link.DelayLoadDLLs.Add($"{linkFlag[10..].Trim('"')}");
						break;
					case var _ when linkFlag.StartsWith("driver", StringComparison.InvariantCultureIgnoreCase):
					{
						var driverFlag = linkFlag.Length > 6 ? linkFlag[6..] : new();
						Program.currentConfig!.Properties.Link.Driver = driverFlag switch
						{
							{ Length: 0 } => "Driver",
							var _ when driverFlag.Equals(":uponly", StringComparison.InvariantCultureIgnoreCase) => "UpOnly",
							var _ when driverFlag.Equals(":wdm", StringComparison.InvariantCultureIgnoreCase) => "WDM",
							_ => Program.ThrowInvalidFlag<string>("link.exe", driverFlag, lineNumber, "/driver")
						};
						break;
					}
					case { Length: > 6 } when linkFlag.StartsWith("entry:", StringComparison.InvariantCultureIgnoreCase):
						Program.currentConfig!.Properties.Link.EntryPointSymbol = $"{linkFlag[6..].Trim('"')}";
						break;
					case var _ when linkFlag.StartsWith("fixed", StringComparison.InvariantCultureIgnoreCase):
					{
						var fixedFlag = linkFlag.Length > 5 ? linkFlag[5..] : new();
						Program.currentConfig!.Properties.Link.FixedBaseAddress = fixedFlag switch
						{
							{ Length: 0 } => true,
							var _ when fixedFlag.Equals(":no", StringComparison.InvariantCultureIgnoreCase) => false,
							_ => Program.ThrowInvalidFlag<bool>("link.exe", fixedFlag, lineNumber, "/fixed")
						};
						break;
					}
					case var _ when linkFlag.StartsWith("force", StringComparison.InvariantCultureIgnoreCase):
					{
						var forceFlag = linkFlag.Length > 5 ? linkFlag[5..] : new();
						Program.currentConfig!.Properties.Link.ForceFileOutput = forceFlag switch
						{
							{ Length: 0 } => "Enabled",
							var _ when forceFlag.Equals(":multiple", StringComparison.InvariantCultureIgnoreCase) =>
								"MultiplyDefinedSymbolOnly",
							var _ when forceFlag.Equals(":unresolved", StringComparison.InvariantCultureIgnoreCase) =>
								"UndefinedSymbolOnly",
							_ => Program.ThrowInvalidFlag<string>("link.exe", forceFlag, lineNumber, "/force")
						};
						break;
					}
					case { Length: > 5 } when linkFlag.StartsWith("heap:", StringComparison.InvariantCultureIgnoreCase):
					{
						// Maybe TODO: Check if the arguments are integers or not
						var heapFlag = linkFlag[5..];
						int comma = heapFlag.IndexOf(',');
						Program.currentConfig!.Properties.Link.HeapReserveSize =
							$"{(comma == -1 ? heapFlag : heapFlag[..comma]).Trim('"')}";
						if (comma != -1)
							Program.currentConfig!.Properties.Link.HeapCommitSize = $"{heapFlag[(comma + 1)..].Trim('"')}";
						break;
					}
					case { Length: > 7 } when linkFlag.StartsWith("implib:", StringComparison.InvariantCultureIgnoreCase):
					{
						var library = linkFlag[7..].Trim('"');
						// Ignoring if the library was $(OutDir)$(TargetName).lib, as this is the default
						if (library is not "$(OutDir)$(TargetName).lib")
							Program.currentConfig!.Properties.Link.ImportLibrary = $"{library}";
						break;
					}
					case { Length: > 8 } when linkFlag.StartsWith("include:", StringComparison.InvariantCultureIgnoreCase):
						_ = Program.currentConfig!.Properties.Link.ForceSymbolReferences.Add($"{linkFlag[8..].Trim('"')}");
						break;
					case { Length: > 12 } when linkFlag.StartsWith("incremental:", StringComparison.InvariantCultureIgnoreCase):
					{
						var incrementalFlag = linkFlag[12..];
						switch (incrementalFlag)
						{
							case var _ when incrementalFlag.Equals("yes", StringComparison.InvariantCultureIgnoreCase):
								// Ignoring if UseDebugLibraries is set to true, as this is the default
								if (Program.currentConfig!.Properties.UseDebugLibraries != true)
									Program.currentConfig!.Properties.Link.LinkIncremental = true;
								break;
							case var _ when incrementalFlag.Equals("no", StringComparison.InvariantCultureIgnoreCase):
								Program.currentConfig!.Properties.Link.LinkIncremental = false;
								break;
							default:
								throw Program.ThrowInvalidFlag("link.exe", incrementalFlag, lineNumber, "/incremental:");
						}
						break;
					}
					case var _ when linkFlag.Equals("largeaddressaware", StringComparison.InvariantCultureIgnoreCase):
					{
						var laaFlag = linkFlag.Length > 17 ? linkFlag[17..] : new();
						Program.currentConfig!.Properties.Link.LargeAddressAware = laaFlag switch
						{
							{ Length: 0 } => true,
							var _ when laaFlag.Equals(":no", StringComparison.InvariantCultureIgnoreCase) => false,
							_ => Program.ThrowInvalidFlag<bool>("link.exe", laaFlag, lineNumber, "/largeaddressaware")
						};
						break;
					}
					case { Length: > 8 } when linkFlag.StartsWith("libpath:", StringComparison.InvariantCultureIgnoreCase):
						_ = Program.currentConfig!.Properties.Link.AdditionalLibraryDirectories.Add($"{linkFlag[8..].Trim('"')}");
						break;
					case { Length: > 8 } when linkFlag.StartsWith("machine:", StringComparison.InvariantCultureIgnoreCase):
					{
						var machineFlag = linkFlag[8..];
						switch (machineFlag)
						{
							case var _ when machineFlag.Equals("arm", StringComparison.InvariantCultureIgnoreCase):
								Program.currentConfig!.Properties.Link.TargetMachine = "MachineARM";
								break;
							case var _ when machineFlag.Equals("ix86", StringComparison.InvariantCultureIgnoreCase):
							case var _ when machineFlag.Equals("i386", StringComparison.InvariantCultureIgnoreCase):
							case var _ when machineFlag.Equals("x86", StringComparison.InvariantCultureIgnoreCase):
								// This might be wrong to use for ix86
								// i386 is here even though it isn't in the docs because the VS 6.0 IDE seems to set that
								// x86 is here because I've encountered an edited (probably) project containing it
								// Igorning if Platform is Win32, as this is the default
								if (Program.currentConfig!.Platform != "Win32")
									Program.currentConfig!.Properties.Link.TargetMachine = "MachineX86";
								break;
							case var _ when machineFlag.Equals("mips", StringComparison.InvariantCultureIgnoreCase):
								Program.currentConfig!.Properties.Link.TargetMachine = "MachineMIPS";
								break;
							case var _ when machineFlag.Equals("mips16", StringComparison.InvariantCultureIgnoreCase):
								Program.currentConfig!.Properties.Link.TargetMachine = "MachineMIPS16";
								break;
							case var _ when machineFlag.Equals("sh4", StringComparison.InvariantCultureIgnoreCase):
								Program.currentConfig!.Properties.Link.TargetMachine = "MachineSH4";
								break;
							case var _ when machineFlag.Equals("alpha", StringComparison.InvariantCultureIgnoreCase):
							case var _ when machineFlag.Equals("mipsr41xx", StringComparison.InvariantCultureIgnoreCase):
							case var _ when machineFlag.Equals("ppc", StringComparison.InvariantCultureIgnoreCase):
							case var _ when machineFlag.Equals("sh3", StringComparison.InvariantCultureIgnoreCase):
								break; // Ignoring these flags as they no longer exists in VS 2015 and later
							default:
								throw Program.ThrowInvalidFlag("link.exe", machineFlag, lineNumber, "/machine:");
						}
						break;
					}
					case var _ when linkFlag.StartsWith("map", StringComparison.InvariantCultureIgnoreCase):
						if (linkFlag.Length > 8 && linkFlag.StartsWith("mapinfo:", StringComparison.InvariantCultureIgnoreCase))
						{
							var mapinfoFlag = linkFlag[8..];
							switch (mapinfoFlag)
							{
								case var _ when mapinfoFlag.Equals("exports", StringComparison.InvariantCultureIgnoreCase):
									Program.currentConfig!.Properties.Link.MapExports = true;
									break;
								case var _ when mapinfoFlag.Equals("fixups", StringComparison.InvariantCultureIgnoreCase):
								case var _ when mapinfoFlag.Equals("lines", StringComparison.InvariantCultureIgnoreCase):
									break; // Ignoring these flags as they no longer exists in VS 2015 and later
								default:
									throw Program.ThrowInvalidFlag("link.exe", mapinfoFlag, lineNumber, "/mapinfo:");
							}
						}
						else
						{
							Program.currentConfig!.Properties.Link.GenerateMapFile = true;
							if (linkFlag.Length > 3)
								Program.currentConfig!.Properties.Link.MapFileName = linkFlag[3] == ':' ? $"{linkFlag[4..].Trim('"')}" :
									Program.ThrowInvalidFlag<string>("link.exe", linkFlag[3..], lineNumber, "/map");
						}
						break;
					case { Length: > 6 } when linkFlag.StartsWith("merge:", StringComparison.InvariantCultureIgnoreCase):
						Program.currentConfig!.Properties.Link.MergeSections = $"{linkFlag[6..].Trim('"')}";
						break;
					case var _ when linkFlag.StartsWith("nodefaultlib", StringComparison.InvariantCultureIgnoreCase):
						switch (linkFlag)
						{
							case { Length: 12 }:
								Program.currentConfig!.Properties.Link.IgnoreAllDefaultLibraries = true;
								break;
							case { Length: > 13 } when linkFlag[12] == ':':
								_ = Program.currentConfig!.Properties.Link.IgnoreSpecificDefaultLibraries.
									Add($"{linkFlag[13..].Trim('"')}");
								break;
							default:
								throw Program.ThrowInvalidFlag("link.exe", linkFlag[12..], lineNumber, "/nodefaultlib");
						}
						break;
					case var _ when linkFlag.Equals("noentry", StringComparison.InvariantCultureIgnoreCase):
						Program.currentConfig!.Properties.Link.NoEntryPoint = true;
						break;
					case var _ when linkFlag.Equals("nologo", StringComparison.InvariantCultureIgnoreCase):
						hadNoLogo = true;
						break;
					case { Length: > 4 } when linkFlag.StartsWith("opt:", StringComparison.InvariantCultureIgnoreCase):
					{
						var optFlag = linkFlag[4..];
						switch (optFlag)
						{
							case var _ when optFlag.StartsWith("icf", StringComparison.InvariantCultureIgnoreCase):
								switch (optFlag)
								{
									case { Length: 3 }:
										Program.currentConfig!.Properties.Link.EnableCOMDATFolding = true;
										break;
									case { Length: > 3 } when optFlag[3] == ',':
										_ = Program.currentConfig!.Properties.Link.AdditionalOptions.Add($"{flag}".Replace(',', '='));
										break;
									default:
										throw Program.ThrowInvalidFlag("link.exe", optFlag, lineNumber, "/opt:");
								}
								break;
							case var _ when optFlag.Equals("noicf", StringComparison.InvariantCultureIgnoreCase):
								Program.currentConfig!.Properties.Link.EnableCOMDATFolding = false;
								break;
							case var _ when optFlag.Equals("noref", StringComparison.InvariantCultureIgnoreCase):
								Program.currentConfig!.Properties.Link.OptimizeReferences = false;
								break;
							case var _ when optFlag.Equals("ref", StringComparison.InvariantCultureIgnoreCase):
								Program.currentConfig!.Properties.Link.OptimizeReferences = true;
								break;
							case var _ when optFlag.Equals("nowin98", StringComparison.InvariantCultureIgnoreCase):
							case var _ when optFlag.Equals("win98", StringComparison.InvariantCultureIgnoreCase):
								break; // Ignoring these flags as they no longer exists in VS 2015 and later
							default:
								throw Program.ThrowInvalidFlag("link.exe", optFlag, lineNumber, "/opt:");
						}
						break;
					}
					case { Length: > 7 } when linkFlag.StartsWith("order:@", StringComparison.InvariantCultureIgnoreCase):
						Program.currentConfig!.Properties.Link.FunctionOrder = $"{linkFlag[7..].Trim('"')}";
						break;
					case { Length: > 4 } when linkFlag.StartsWith("out:", StringComparison.InvariantCultureIgnoreCase):
					{
						var filename = linkFlag[4..].Trim('"');
						// Ignoring if the filename was $(OutDir)$(TargetName)$(TargetExt), as this is the default
						if (filename is not "$(OutDir)$(TargetName)$(TargetExt)")
							Program.currentConfig!.Properties.Link.OutputFile = $"{filename}";
						break;
					}
					case { Length: > 4 } when linkFlag.StartsWith("pdb:", StringComparison.InvariantCultureIgnoreCase):
					{
						var filename = linkFlag[4..].Trim('"');
						// Ignoring if the "filename" was none, as this no longer exists in VS 2015 and later
						if (!filename.Equals("none", StringComparison.InvariantCultureIgnoreCase))
						{
							// Ignoring if the filename was $(OutDir)$(TargetName).pdb, as this is the default
							if (filename is not "$(OutDir)$(TargetName).pdb")
								Program.currentConfig!.Properties.Link.ProgramDatabaseFile = $"{filename}";
						}
						break;
					}
					case var _ when linkFlag.Equals("profile", StringComparison.InvariantCultureIgnoreCase):
						Program.currentConfig!.Properties.Link.Profile = true;
						break;
					case var _ when linkFlag.Equals("release", StringComparison.InvariantCultureIgnoreCase):
						Program.currentConfig!.Properties.Link.SetChecksum = true;
						break;
					case { Length: > 8 } when linkFlag.StartsWith("section:", StringComparison.InvariantCultureIgnoreCase):
						Program.currentConfig!.Properties.Link.SpecifySectionAttributes = $"{linkFlag[8..].Trim('"')}";
						break;
					case { Length: > 6 } when linkFlag.StartsWith("stack:", StringComparison.InvariantCultureIgnoreCase):
					{
						// Maybe TODO: Check if the arguments are integers or not
						var stackFlag = linkFlag[6..];
						int comma = stackFlag.IndexOf(',');
						Program.currentConfig!.Properties.Link.StackReserveSize =
							$"{(comma == -1 ? stackFlag : stackFlag[..comma]).Trim('"')}";
						if (comma != -1)
							Program.currentConfig!.Properties.Link.StackCommitSize = $"{stackFlag[(comma + 1)..].Trim('"')}";
						break;
					}
					case { Length: > 5 } when linkFlag.StartsWith("stub:", StringComparison.InvariantCultureIgnoreCase):
						Program.currentConfig!.Properties.Link.MSDOSStubFileName = $"{linkFlag[5..].Trim('"')}";
						break;
					case { Length: > 10 } when linkFlag.StartsWith("subsystem:", StringComparison.InvariantCultureIgnoreCase):
					{
						var subsystemFlag = linkFlag[10..];
						int comma = subsystemFlag.IndexOf(',');
						if (comma != -1)
						{
							subsystemFlag = subsystemFlag[..comma];
							_ = subsystemFlag.Equals("console", StringComparison.InvariantCultureIgnoreCase) ||
								subsystemFlag.Equals("native", StringComparison.InvariantCultureIgnoreCase) ||
								subsystemFlag.Equals("posix", StringComparison.InvariantCultureIgnoreCase) ||
								subsystemFlag.Equals("windows", StringComparison.InvariantCultureIgnoreCase) ||
								subsystemFlag.Equals("windowsce", StringComparison.InvariantCultureIgnoreCase) ?
								Program.currentConfig!.Properties.Link.AdditionalOptions.Add($"{flag}") :
								throw Program.ThrowInvalidFlag("link.exe", subsystemFlag, lineNumber, "/subsystem:");
						}
						else
							Program.currentConfig!.Properties.Link.SubSystem = subsystemFlag switch
							{
								var _ when subsystemFlag.Equals("console", StringComparison.InvariantCultureIgnoreCase) => "Console",
								var _ when subsystemFlag.Equals("native", StringComparison.InvariantCultureIgnoreCase) => "Native",
								var _ when subsystemFlag.Equals("posix", StringComparison.InvariantCultureIgnoreCase) => "POSIX",
								var _ when subsystemFlag.Equals("windows", StringComparison.InvariantCultureIgnoreCase) => "Windows",
								var _ when subsystemFlag.Equals("windowsce", StringComparison.InvariantCultureIgnoreCase) =>
									"WindowsCE",
								_ => Program.ThrowInvalidFlag<string>("link.exe", subsystemFlag, lineNumber, "/subsystem:")
							};
						break;
					}
					case { Length: > 8 } when linkFlag.StartsWith("swaprun:", StringComparison.InvariantCultureIgnoreCase):
					{
						var swaprunFlag = linkFlag[8..];
						switch (swaprunFlag)
						{
							case var _ when swaprunFlag.Equals("cd", StringComparison.InvariantCultureIgnoreCase):
								Program.currentConfig!.Properties.Link.SwapRunFromCD = true;
								break;
							case var _ when swaprunFlag.Equals("net", StringComparison.InvariantCultureIgnoreCase):
								Program.currentConfig!.Properties.Link.SwapRunFromNET = true;
								break;
							default:
								throw Program.ThrowInvalidFlag("link.exe", swaprunFlag, lineNumber, "/swaprun:");
						}
						break;
					}
					case var _ when linkFlag.StartsWith("verbose", StringComparison.InvariantCultureIgnoreCase):
					{
						var verboseFlag = linkFlag.Length > 7 ? linkFlag[7..] : new();
						Program.currentConfig!.Properties.Link.ShowProgress = verboseFlag switch
						{
							{ Length: 0 } => "LinkVerbose",
							var _ when verboseFlag.Equals(":lib", StringComparison.InvariantCultureIgnoreCase) => "LinkVerboseLib",
							_ => Program.ThrowInvalidFlag<string>("link.exe", verboseFlag, lineNumber, "/verbose")
						};
						break;
					}
					case { Length: > 8 } when linkFlag.StartsWith("version:", StringComparison.InvariantCultureIgnoreCase):
						Program.currentConfig!.Properties.Link.Version = $"{linkFlag[8..].Trim('"')}";
						break;
					case { Length: > 11 } when linkFlag.StartsWith("defaultlib:", StringComparison.InvariantCultureIgnoreCase):
					case { Length: > 7 } when linkFlag.StartsWith("export:", StringComparison.InvariantCultureIgnoreCase):
						_ = Program.currentConfig!.Properties.Link.AdditionalOptions.Add($"{flag}");
						break;
					case var _ when linkFlag.Equals("dll", StringComparison.InvariantCultureIgnoreCase):
						break; // Ignored as it isn't necessary, ConfigurationType much earlier is usually set to DynamicLibrary
					case var _ when linkFlag.StartsWith("comment:", StringComparison.InvariantCultureIgnoreCase):
					case var _ when linkFlag.StartsWith("debugtype:", StringComparison.InvariantCultureIgnoreCase):
					case var _ when linkFlag.Equals("exetype:dynamic", StringComparison.InvariantCultureIgnoreCase):
					case var _ when linkFlag.StartsWith("gpsize:", StringComparison.InvariantCultureIgnoreCase):
					case var _ when linkFlag.Equals("link50compat", StringComparison.InvariantCultureIgnoreCase):
					case var _ when linkFlag.StartsWith("pdbtype:", StringComparison.InvariantCultureIgnoreCase):
					case var _ when linkFlag.Equals("vxd", StringComparison.InvariantCultureIgnoreCase):
					case var _ when linkFlag.StartsWith("warn:", StringComparison.InvariantCultureIgnoreCase):
					case var _ when linkFlag.StartsWith("windowsce:", StringComparison.InvariantCultureIgnoreCase):
					case var _ when linkFlag.Equals("ws:aggresive", StringComparison.InvariantCultureIgnoreCase):
						break; // Ignored because they no longer exist in VS 2015 and later
					default:
						throw Program.ThrowInvalidFlag("link.exe", flag, lineNumber);
				}
			}
			else
				_ = Program.currentConfig!.Properties.Link.AdditionalDependencies.Add($"{flag}");
		}
		// This is being done like this because SuppressStartupBanner defaults to true
		if (Program.currentFile is null && !hadNoLogo)
			Program.currentConfig!.Properties.Link.SuppressStartupBanner = false;
	}

	/// <summary>
	/// Process a line for the librarian.
	/// </summary>
	/// <param name="line">The contents of the line.</param>
	/// <param name="lineNumber">The line number of the line.</param>
	static void ProcessLib(ReadOnlySpan<char> line, int lineNumber)
	{
		bool hadNoLogo = false;
		line = line[(line.Contains("BASE", StringComparison.InvariantCulture) ? 17 : 12)..];
		int spaces = line.Count(' ');
		Span<Range> ranges = stackalloc Range[spaces + 1];
		int numRanges = line.Split(ranges, ' ', StringSplitOptions.RemoveEmptyEntries);
		foreach (var range in ranges[..numRanges])
		{
			var flag = line[range];
			var librarianFlag = flag;
			if (librarianFlag[0] is '/' or '-')
			{
				librarianFlag = librarianFlag[1..];
				switch (librarianFlag)
				{
					case var _ when librarianFlag.StartsWith("def", StringComparison.InvariantCultureIgnoreCase):
						switch (librarianFlag)
						{
							case { Length: 3 }:
								_ = Program.currentConfig!.Properties.Lib.AdditionalOptions.Add($"{flag}");
								break;
							case { Length: > 4 } when librarianFlag[3] == ':':
								Program.currentConfig!.Properties.Lib.ModuleDefinitionFile = $"{librarianFlag[4..].Trim('"')}";
								break;
							default:
								throw Program.ThrowInvalidFlag("lib.exe", librarianFlag[3..], lineNumber, "/def");
						}
						break;
					case { Length: > 7 } when librarianFlag.StartsWith("export:", StringComparison.InvariantCultureIgnoreCase):
						_ = Program.currentConfig!.Properties.Lib.ExportNamedFunctions.Add($"{librarianFlag[7..].Trim('"')}");
						break;
					case { Length: > 8 } when librarianFlag.StartsWith("include:", StringComparison.InvariantCultureIgnoreCase):
						Program.currentConfig!.Properties.Lib.ForceSymbolReferences = $"{librarianFlag[8..].Trim('"')}";
						break;
					case { Length: > 8 } when librarianFlag.StartsWith("libpath:", StringComparison.InvariantCultureIgnoreCase):
						_ = Program.currentConfig!.Properties.Lib.AdditionalLibraryDirectories.Add($"{librarianFlag[8..].Trim('"')}");
						break;
					case { Length: > 8 } when librarianFlag.StartsWith("machine:", StringComparison.InvariantCultureIgnoreCase):
					{
						var machineFlag = librarianFlag[8..];
						switch (machineFlag)
						{
							case var _ when machineFlag.Equals("arm", StringComparison.InvariantCultureIgnoreCase):
								Program.currentConfig!.Properties.Lib.TargetMachine = "MachineARM";
								break;
							case var _ when machineFlag.Equals("ix86", StringComparison.InvariantCultureIgnoreCase):
							case var _ when machineFlag.Equals("i386", StringComparison.InvariantCultureIgnoreCase):
							case var _ when machineFlag.Equals("x86", StringComparison.InvariantCultureIgnoreCase):
								// This might be wrong to use for ix86
								// i386 is here even though it isn't in the docs because the VS 6.0 IDE seems to set that
								// x86 is here because I've encountered an edited (probably) project containing it
								// Ignoring if Platform is Win32, as this is the default
								if (Program.currentConfig!.Platform != "Win32")
									Program.currentConfig!.Properties.Lib.TargetMachine = "MachineX86";
								break;
							case var _ when machineFlag.Equals("mips", StringComparison.InvariantCultureIgnoreCase):
								Program.currentConfig!.Properties.Lib.TargetMachine = "MachineMIPS";
								break;
							case var _ when machineFlag.Equals("mips16", StringComparison.InvariantCultureIgnoreCase):
								Program.currentConfig!.Properties.Lib.TargetMachine = "MachineMIPS16";
								break;
							case var _ when machineFlag.Equals("sh4", StringComparison.InvariantCultureIgnoreCase):
								Program.currentConfig!.Properties.Lib.TargetMachine = "MachineSH4";
								break;
							case var _ when machineFlag.Equals("alpha", StringComparison.InvariantCultureIgnoreCase):
							case var _ when machineFlag.Equals("mipsr41xx", StringComparison.InvariantCultureIgnoreCase):
							case var _ when machineFlag.Equals("ppc", StringComparison.InvariantCultureIgnoreCase):
							case var _ when machineFlag.Equals("sh3", StringComparison.InvariantCultureIgnoreCase):
								break; // Ignoring these flags as they no longer exists in VS 2015 and later
							default:
								throw Program.ThrowInvalidFlag("lib.exe", machineFlag, lineNumber, "/machine:");
						}
						break;
					}
					case { Length: > 5 } when librarianFlag.StartsWith("name:", StringComparison.InvariantCultureIgnoreCase):
						Program.currentConfig!.Properties.Lib.Name = $"{librarianFlag[5..].Trim('"')}";
						break;
					case var _ when librarianFlag.StartsWith("nodefaultlib", StringComparison.InvariantCultureIgnoreCase):
						switch (librarianFlag)
						{
							case { Length: 12 }:
								Program.currentConfig!.Properties.Lib.IgnoreAllDefaultLibraries = true;
								break;
							case { Length: > 13 } when librarianFlag[12] == ':':
								_ = Program.currentConfig!.Properties.Lib.IgnoreSpecificDefaultLibraries.
									Add($"{librarianFlag[13..].Trim('"')}");
								break;
							default:
								throw Program.ThrowInvalidFlag("lib.exe", librarianFlag[12..], lineNumber, "/nodefaultlib");
						}
						break;
					case var _ when librarianFlag.Equals("nologo", StringComparison.InvariantCultureIgnoreCase):
						hadNoLogo = true;
						break;
					case { Length: > 4 } when librarianFlag.StartsWith("out:", StringComparison.InvariantCultureIgnoreCase):
					{
						var filename = librarianFlag[4..].Trim('"');
						// Ignoring if the filename was $(OutDir)$(TargetName)$(TargetExt), as this is the default
						if (filename is not "$(OutDir)$(TargetName)$(TargetExt)")
							Program.currentConfig!.Properties.Lib.OutputFile = $"{filename}";
						break;
					}
					case { Length: > 7 } when librarianFlag.StartsWith("remove:", StringComparison.InvariantCultureIgnoreCase):
						_ = Program.currentConfig!.Properties.Lib.RemoveObjects.Add($"{librarianFlag[7..].Trim('"')}");
						break;
					case { Length: > 10 } when librarianFlag.StartsWith("subsystem:", StringComparison.InvariantCultureIgnoreCase):
					{
						var subsystemFlag = librarianFlag[10..];
						int comma = subsystemFlag.IndexOf(',');
						if (comma != -1)
						{
							subsystemFlag = subsystemFlag[..comma];
							_ = subsystemFlag.Equals("console", StringComparison.InvariantCultureIgnoreCase) ||
								subsystemFlag.Equals("native", StringComparison.InvariantCultureIgnoreCase) ||
								subsystemFlag.Equals("posix", StringComparison.InvariantCultureIgnoreCase) ||
								subsystemFlag.Equals("windows", StringComparison.InvariantCultureIgnoreCase) ||
								subsystemFlag.Equals("windowsce", StringComparison.InvariantCultureIgnoreCase) ?
								Program.currentConfig!.Properties.Lib.AdditionalOptions.Add($"{flag}") :
								throw Program.ThrowInvalidFlag("lib.exe", subsystemFlag, lineNumber, "/subsystem:");
						}
						else
							Program.currentConfig!.Properties.Lib.SubSystem = subsystemFlag switch
							{
								var _ when subsystemFlag.Equals("console", StringComparison.InvariantCultureIgnoreCase) => "Console",
								var _ when subsystemFlag.Equals("native", StringComparison.InvariantCultureIgnoreCase) => "Native",
								var _ when subsystemFlag.Equals("posix", StringComparison.InvariantCultureIgnoreCase) => "POSIX",
								var _ when subsystemFlag.Equals("windows", StringComparison.InvariantCultureIgnoreCase) => "Windows",
								var _ when subsystemFlag.Equals("windowsce", StringComparison.InvariantCultureIgnoreCase) => "WindowsCE",
								_ => Program.ThrowInvalidFlag<string>("lib.exe", subsystemFlag, lineNumber, "/subsystem:")
							};
						break;
					}
					case var _ when librarianFlag.Equals("verbose", StringComparison.InvariantCultureIgnoreCase):
						Program.currentConfig!.Properties.Lib.Verbose = true;
						break;
					case { Length: > 8 } when librarianFlag.StartsWith("extract:", StringComparison.InvariantCultureIgnoreCase):
						_ = Program.currentConfig!.Properties.Lib.AdditionalOptions.Add($"{flag}");
						break;
					case var _ when librarianFlag.StartsWith("list", StringComparison.InvariantCultureIgnoreCase):
						break; // Ignoring as there is really no reason this would ever be in an IDE project file
					case var _ when librarianFlag.Equals("convert", StringComparison.InvariantCultureIgnoreCase):
					case var _ when librarianFlag.StartsWith("debugtype:", StringComparison.InvariantCultureIgnoreCase):
					case var _ when librarianFlag.Equals("link50compat", StringComparison.InvariantCultureIgnoreCase):
						break; // Ignored because they no longer exist in VS 2015 and later
					default:
						throw Program.ThrowInvalidFlag("lib.exe", flag, lineNumber);
				}
			}
			else
				_ = Program.currentConfig!.Properties.Lib.AdditionalDependencies.Add($"{flag}");
		}
		// This is being done like this because SuppressStartupBanner defaults to true
		if (Program.currentFile is null && !hadNoLogo)
			Program.currentConfig!.Properties.Lib.SuppressStartupBanner = false;
	}

	/// <summary>
	/// Process a line for the resource compiler.
	/// </summary>
	/// <param name="resourceFlags">The contents of the line.</param>
	/// <param name="lineNumber">The line number of the line.</param>
	static void ProcessRC(ReadOnlySpan<char> resourceFlags, int lineNumber)
	{
		resourceFlags = resourceFlags[(resourceFlags.Contains("BASE", StringComparison.InvariantCulture) ? 15 : 10)..];
		int spaces = resourceFlags.Count(' ');
		Span<Range> ranges = stackalloc Range[spaces + 1];
		int numResourceFlags = resourceFlags.Split(ranges, ' ', StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < numResourceFlags; ++i)
		{
			var resourceFlag = resourceFlags[ranges[i]];
			if (resourceFlag[0] is '/' or '-')
			{
				resourceFlag = resourceFlag[1..];
				switch (resourceFlag)
				{
					case var _ when resourceFlag.StartsWith("c", StringComparison.InvariantCultureIgnoreCase):
						Program.multipleClCompile.AdditionalOptionsAdd($"{resourceFlag}{(resourceFlag.Length == 1 ? "" :
							resourceFlags[ranges[++i]])}");
						break;
					case var _ when resourceFlag.StartsWith("d", StringComparison.InvariantCultureIgnoreCase):
						Program.multipleResourceCompile.PreprocessorDefinitionsAdd($"{(resourceFlag.Length > 1 ? resourceFlag[1..] :
							resourceFlags[ranges[++i]]).Trim('"')}");
						break;
					case var _ when resourceFlag.StartsWith("fo", StringComparison.InvariantCultureIgnoreCase):
					{
						var filename = (resourceFlag.Length > 2 ? resourceFlag[2..] : resourceFlags[ranges[++i]]).Trim('"');
						// Ignoring if the filename was $(IntDir)%(Filename).res, as this is the default
						if (filename is not "$(IntDir)%(Filename).res")
							Program.multipleResourceCompile.ResourceOutputFileName = $"{filename}";
						break;
					}
					case var _ when resourceFlag.StartsWith("i", StringComparison.InvariantCultureIgnoreCase):
						Program.multipleResourceCompile.AdditionalIncludeDirectoriesAdd($"{(resourceFlag.Length > 1 ? resourceFlag[1..] :
							resourceFlags[ranges[++i]]).Trim('"')}");
						break;
					case var _ when resourceFlag.StartsWith("l", StringComparison.InvariantCultureIgnoreCase):
					{
						var culture = resourceFlag.Length > 1 ? resourceFlag[1..] : resourceFlags[ranges[++i]];
						if (culture.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
							culture = culture[2..];
						int cultureNum = int.Parse(culture, NumberStyles.HexNumber);
						// Ignoring if the culture is 0x0409, as this is English and is the default (at least for most systems?)
						// (If this turns out to not be the default for someone else, I'd like to know, I might have to find out how to
						//   determine the default)
						if (cultureNum != 0x0409)
							Program.multipleResourceCompile.Culture = $"0x{cultureNum:x4}";
						break;
					}
					case var _ when resourceFlag.Equals("n", StringComparison.InvariantCultureIgnoreCase):
						Program.multipleResourceCompile.NullTerminateStrings = true;
						break;
					case var _ when resourceFlag.StartsWith("u", StringComparison.InvariantCultureIgnoreCase):
						Program.multipleResourceCompile.UndefinePreprocessorDefinitionsAdd($"{(resourceFlag.Length > 1 ? resourceFlag[1..] :
							resourceFlags[ranges[++i]]).Trim('"')}");
						break;
					case var _ when resourceFlag.Equals("v", StringComparison.InvariantCultureIgnoreCase):
						Program.multipleResourceCompile.ShowProgress = true;
						break;
					case var _ when resourceFlag.Equals("x", StringComparison.InvariantCultureIgnoreCase):
						Program.multipleResourceCompile.IgnoreStandardIncludePath = true;
						break;
					case var _ when resourceFlag.Equals("w", StringComparison.InvariantCultureIgnoreCase):
						Program.multipleResourceCompile.AdditionalOptionsAdd($"{resourceFlag}");
						break;
					case var _ when resourceFlag.Equals("r", StringComparison.InvariantCultureIgnoreCase):
						break; // Ignored because it is being ignored by rc.exe anyways
					default:
						throw Program.ThrowInvalidFlag("rc.exe", resourceFlag, lineNumber);
				}
			}
			else
				throw Program.ThrowInvalidArgument("rc.exe", resourceFlag, lineNumber);
		}
	}

	/// <summary>
	/// Create the main project file and the filters file for the project.
	/// </summary>
	/// <param name="originalFilename">The filename of the original DSP project file, used as the base of the new filenames.</param>
	static void CreateProjectFiles(string originalFilename)
	{
		// Create XML for VCXPROJ.
		XDocument vcxprojXML = new(new XDeclaration("1.0", "utf-8", null));
		var rootXML = Program.CreateElement("Project",
			new XAttribute("ToolsVersion", "4.0")
		);
		vcxprojXML.Add(rootXML);
		XDocument filtersXML = new(new XDeclaration("1.0", "utf-8", null));
		var filtersRootXML = Program.CreateElement("Project",
			new XAttribute("DefaultTargets", "Build"),
			new XAttribute("ToolsVersion", "4.0")
		);
		filtersXML.Add(filtersRootXML);
		rootXML.Add(Program.CreateItemGroupBlock("ProjectConfigurations",
			Program.projectConfigurations.Select(pc => pc.GetBlock())
		));
		rootXML.Add(Program.CreatePropertyGroupBlock(null, "Globals",
			Program.CreateElement("ProjectGuid", $"{{{$"{Guid.NewGuid()}".ToUpperInvariant()}}}"),
			Program.CreateElement("Keyword", "Win32Proj"),
			Program.CreateElement("RootNamespace", Program.rootNamespace)
		));
		rootXML.Add(Program.CreateImportBlock(@"$(VCTargetsPath)\Microsoft.Cpp.Default.props", null, null));
		foreach (var pc in Program.projectConfigurations)
			rootXML.Add(pc.Properties.GetPrePropsBlock());
		rootXML.Add(Program.CreateImportBlock(@"$(VCTargetsPath)\Microsoft.Cpp.props", null, null));
		foreach (var pc in Program.projectConfigurations)
			rootXML.Add(Program.CreateImportGroupBlock($"'$(Configuration)|$(Platform)'=='{pc.Configuration}|{pc.Platform}'",
				"PropertySheets",
				Program.CreateImportBlock(@"$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props",
					@"exists('$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props')", "LocalAppDataPlatform")
			));
		foreach (var pc in Program.projectConfigurations)
			rootXML.Add(pc.Properties.GetPostPropsBlock());
		foreach (var pc in Program.projectConfigurations)
			rootXML.Add(pc.Properties.GetItemDefinitionBlock());
		var filtersItemGroup = Program.CreateItemGroupBlock(null);
		Program.filters.ForEach(filter =>
		{
			// This will destructively override the extensions for existing well-known groups, as VS 6.0 had a much different set.
			Guid guid;
			if (Program.ExistingGroups.TryGetValue(filter.Include, out var group))
			{
				filter.Extensions = group.Extensions;
				guid = group.GUID;
			}
			else
				guid = Guid.NewGuid();
			var filterBlock = filter.GetBlock(true);
			filterBlock.Add(Program.CreateElement("UniqueIdentifier", $"{{{$"{guid}".ToUpperInvariant()}}}"));
			filtersItemGroup.Add(filterBlock);
		});
		filtersRootXML.Add(filtersItemGroup);
		Program.GetFiles<ClCompile>(rootXML, filtersRootXML, Program.files);
		Program.GetFiles<None>(rootXML, filtersRootXML, Program.files);
		Program.GetFiles<ClInclude>(rootXML, filtersRootXML, Program.files);
		Program.GetFiles<ResourceCompile>(rootXML, filtersRootXML, Program.files);
		Program.GetFiles<Image>(rootXML, filtersRootXML, Program.files);
		Program.GetFiles<Text>(rootXML, filtersRootXML, Program.files);
		rootXML.Add(Program.CreateImportBlock(@"$(VCTargetsPath)\Microsoft.Cpp.targets", null, null));

		// Save VCXPROJ.
		string vcxprojFilename =
			$"{Path.Combine(Path.GetDirectoryName(originalFilename)!, Path.GetFileNameWithoutExtension(originalFilename))}.vcxproj";
		vcxprojXML.Save(vcxprojFilename);
		filtersXML.Save($"{vcxprojFilename}.filters");
	}

	static void Main(string[] args)
	{
		if (args.Length < 1)
		{
			Console.Error.WriteLine("Error: Need DSP file as argument.");
			return;
		}

		// Read DSP
		bool validFile = false;
		bool inCustomBuild = false;
		bool processsCustomBuild = false;
		bool inTarget = false;
		int lineNumber = 0;
		foreach (string line in File.ReadAllLines(args[0]))
		{
			++lineNumber;
			if (!string.IsNullOrWhiteSpace(line))
				switch (line)
				{
					case var _ when line.StartsWith("# Microsoft Developer Studio Project File", StringComparison.InvariantCulture):
					{
						// Should be the first line of the file, contains the root namespace, and we mark this as a valid file
						var rootNamespace = line.AsSpan(50);
						Program.rootNamespace = $"{rootNamespace[..(rootNamespace.IndexOf(' ') - 1)]}";
						validFile = true;
						break;
					}
					case var _ when line.StartsWith("# TARGTYPE", StringComparison.InvariantCulture):
					{
						// Contains the target type, only the below 4 types are handled
						int lastSpace = line.LastIndexOf(' ');
						Program.ConfigurationType = int.Parse(line.AsSpan(lastSpace + 3), NumberStyles.HexNumber) switch
						{
							0x0101 or 0x0103 => "Application",
							0x0102 => "DynamicLibrary",
							0x0104 => "StaticLibrary",
							_ => Program.ThrowInvalid<string>("target", "type", line.AsSpan(lastSpace + 3), lineNumber)
						};
						break;
					}
					case var _ when line.StartsWith("!MESSAGE \"", StringComparison.InvariantCulture):
					{
						// Although designated a message, the messages which start with a quote contain the project configurations
						int dash = line.IndexOf(" - ");
						var configuration = line.AsSpan()[(dash + 3)..line.IndexOf('"', dash)];
						int space = configuration.IndexOf(' ');
						Program.projectConfigurations.Add(new($"{configuration[(space + 1)..]}", $"{configuration[..space]}"));
						break;
					}
					case var _ when line.StartsWith("!IF  \"$(CFG)\"", StringComparison.InvariantCulture):
					case var _ when line.StartsWith("!ELSEIF  \"$(CFG)\"", StringComparison.InvariantCulture):
					{
						// This is the start of a conditional configuration block
						string configuration = line[(line.IndexOf(" - ") + 3)..^1];
						Program.currentConfig = Program.projectConfigurations[configuration];
						if (Program.currentFile is null)
						{
							Program.multipleClCompile.Objects =
							[
								Program.currentConfig.Properties.ClCompile
							];
							Program.multipleResourceCompile.Objects =
							[
								Program.currentConfig.Properties.ResourceCompile
							];
						}
						else
						{
							Program.multipleClCompile.Objects =
							[
								(Program.currentFile.Configurations[configuration] as ClCompile)!
							];
							Program.multipleResourceCompile.Objects =
							[
								(Program.currentFile.Configurations[configuration] as ResourceCompile)!
							];
						}
						break;
					}
					case var _ when line.StartsWith("!ENDIF", StringComparison.InvariantCulture) && Program.currentFile is not null:
						// This is the end of a conditional configuration block
						Program.multipleClCompile.Objects = [.. Program.currentFile.Configurations.Values.OfType<ClCompile>()];
						Program.multipleResourceCompile.Objects = [.. Program.currentFile.Configurations.Values.OfType<ResourceCompile>()];
						break;
					case { Length: > 7 } when line.StartsWith("# PROP", StringComparison.InvariantCulture):
						// General properties for the project
						Program.ProcessProps(line, lineNumber);
						break;
					case { Length: > 10 } when line.StartsWith("# ADD CPP", StringComparison.InvariantCulture):
					case { Length: > 15 } when line.StartsWith("# ADD BASE CPP", StringComparison.InvariantCulture):
						// C/C++ Compiler properties
						Program.ProcessCPP(line, lineNumber);
						break;
					case { Length: > 13 } when line.StartsWith("# ADD LINK32", StringComparison.InvariantCulture):
					case { Length: > 18 } when line.StartsWith("# ADD BASE LINK32", StringComparison.InvariantCulture):
						// Linker properties
						Program.ProcessLink(line, lineNumber);
						break;
					case { Length: > 12 } when line.StartsWith("# ADD LIB32", StringComparison.InvariantCulture):
					case { Length: > 17 } when line.StartsWith("# ADD BASE LIB32", StringComparison.InvariantCulture):
						// Librarian properties
						Program.ProcessLib(line, lineNumber);
						break;
					case { Length: > 10 } when line.StartsWith("# ADD RSC", StringComparison.InvariantCulture):
					case { Length: > 15 } when line.StartsWith("# ADD BASE RSC", StringComparison.InvariantCulture):
						// Resource Compiler properties
						Program.ProcessRC(line, lineNumber);
						break;
					case var _ when line.StartsWith("PreLink_Desc=", StringComparison.InvariantCulture):
						// Description for Pre-Link build event
						Program.currentConfig!.Properties.PreLinkEvent.Message = line[13..];
						break;
					case var _ when line.StartsWith("PreLink_Cmds=", StringComparison.InvariantCulture):
						// Commands for Pre-Link build event
						Program.currentConfig!.Properties.PreLinkEvent.Command = line[13..].Replace("\t", Environment.NewLine);
						break;
					case var _ when line.StartsWith("PostBuild_Desc=", StringComparison.InvariantCulture):
						// Description for Post-Build build event
						Program.currentConfig!.Properties.PostBuildEvent.Message = line[15..];
						break;
					case var _ when line.StartsWith("PostBuild_Cmds=", StringComparison.InvariantCulture):
						// Commands for Post-Build build event
						Program.currentConfig!.Properties.PostBuildEvent.Command = line[15..].Replace("\t", Environment.NewLine);
						break;
					case var _ when line.StartsWith("# Begin Custom Build", StringComparison.InvariantCulture):
					{
						// The start of a Custom Build Step
						int dash = line.IndexOf(" - ");
						var message = dash == -1 ? new() : line.AsSpan(dash + 3);
						if (message.Length != 0 && message is not "Performing Custom Build Step")
							Program.currentConfig!.Properties.CustomBuildStep.Message = $"{message}";
						inCustomBuild = true;
						break;
					}
					case var _ when processsCustomBuild && line[0] == '\t':
						// Process the actual Custom Build Step, lines starting with a tab are the actual commands
						if (line.Length == 1)
							processsCustomBuild = false; // Sometimes the end of the custom build steps will be a line containing just a tab
						else
							Program.currentConfig!.Properties.CustomBuildStep.Command =
								string.IsNullOrEmpty(Program.currentConfig!.Properties.CustomBuildStep.Command) ? line[1..] :
									$"{Program.currentConfig!.Properties.CustomBuildStep.Command}{Environment.NewLine}{line[1..]}";
						break;
					case var _ when processsCustomBuild:
					{
						// Process the actual Custom Build Step, the line without a tab is the one that contains the output filename
						int colon = line.IndexOf(" : ");
						// Ideally, this should never happen, but in case the build step line happens to not be in the proper format,
						// it will be skipped over as irrelevant
						if (colon > 0)
						{
							if (line[colon - 1] == '"')
								--colon;
							Program.currentConfig!.Properties.CustomBuildStep.Outputs = line[1..colon];
						}
						break;
					}
					case "# End Custom Build":
						inCustomBuild = false;
						break;
					case "# Begin Target":
						// There seems to be only one target in a project file, usually... if there isn't, I'd need to know
						inTarget = true;
						break;
					// The next 2 case blocks probably allocate more often than they should...
					case var _ when line.StartsWith("# Begin Group", StringComparison.InvariantCulture):
					{
						// The start of a filter group
						string groupName = line[14..].Trim('"');
						if (currentFilter is not null)
							groupName = $@"{currentFilter.Include}\{groupName}";
						currentFilter = new(groupName);
						break;
					}
					case "# End Group":
					{
						// The end of a filter group
						int groupLastSlash = currentFilter?.Include.LastIndexOf('\\') ?? -1;
						if (currentFilter is not null)
							Program.filters.Add(currentFilter);
						currentFilter = groupLastSlash != -1 ? new(currentFilter!.Include[..groupLastSlash]) : null;
						break;
					}
					case var _ when inTarget && line.StartsWith("SOURCE=", StringComparison.InvariantCulture):
					{
						// A source file for the build itself (we limit it to the target, as this starting text is also used in the Custom
						// Build Step section)
						string filename = line[7..].Trim('"');
						string extension = Path.GetExtension(filename);
						Program.currentFile = new()
						{
							Configurations = Program.projectConfigurations.ToDictionary(k => $"{k.Platform} {k.Configuration}",
								v => extension switch
								{
									var _ when extension.Equals(".bmp", StringComparison.InvariantCultureIgnoreCase) ||
										extension.Equals(".ico", StringComparison.InvariantCultureIgnoreCase) => new Image(filename),
									var _ when extension.Equals(".c", StringComparison.InvariantCultureIgnoreCase) ||
										extension.Equals(".cpp", StringComparison.InvariantCultureIgnoreCase) ||
										extension.Equals(".cxx", StringComparison.InvariantCultureIgnoreCase) =>
										new ClCompile(filename, v.Properties.Condition),
									var _ when extension.Equals(".h", StringComparison.InvariantCultureIgnoreCase) ||
										extension.Equals(".hpp", StringComparison.InvariantCultureIgnoreCase) ||
										extension.Equals(".hxx", StringComparison.InvariantCultureIgnoreCase) => new ClInclude(filename),
									var _ when extension.Equals(".rc", StringComparison.InvariantCultureIgnoreCase) =>
										new ResourceCompile(filename, v.Properties.Condition),
									var _ when extension.Equals(".txt", StringComparison.InvariantCultureIgnoreCase) => new Text(filename),
									_ => new None(filename)
								})
						};
						foreach (var config in Program.currentFile.Configurations.Values)
							config.Filter = currentFilter;
						Program.multipleClCompile.Objects = [.. Program.currentFile.Configurations.Values.OfType<ClCompile>()];
						Program.multipleResourceCompile.Objects =
							[.. Program.currentFile.Configurations.Values.OfType<ResourceCompile>()];
						break;
					}
					case "# End Source File":
						Program.files.Add(Program.currentFile!);
						break;
				}
			else if (inCustomBuild)
				// A blank line with in the Custom Build section marks the line just before the actual build steps, but it can also be the
				// end of the actual build steps.
				processsCustomBuild = !processsCustomBuild;
		}

		if (validFile)
			Program.CreateProjectFiles(args[0]);
		else
			Console.Error.WriteLine("Error: The given file did not appear to be a DSP file.");
	}
}
