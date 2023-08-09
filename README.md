Primary repository on [GitHub](https://github.com/CyberBotX/DSPtoVCXPROJ).

[.NET 7.0]: https://dotnet.microsoft.com/en-us/download/dotnet/7.0

# DSPtoVCXPROJ

## What is this?

C/C++ projects in Visual Studio use a project file to store the properties to
use on the compiler, linker, etc. Since Visual Studio 2010, the project files
have used the extension `.vcxproj` and use MSBuild. Between Visual Studio .NET
(2002) and Visual Studio 2008, the project files used the extension `.vcproj`
and use VCBuild. Visual Studio 2010 and later can convert these `.vcproj`
project files to `.vcxproj` project files automatically.

But before Visual Studio .NET, we had Visual Studio 6.0 and earlier. They used
project files with the extension `.dsp`. These could be converted to `.vcproj`
by Visual Studio between .NET and 2008, but Visual Studio 2010 dropped this
support and also has no way to convert those `.dsp` project files to `.vcxproj`
project files.

That is where this application comes in. This is an application written in C# to
try to convert `.dsp` project files to `.vcxproj` project files. It tries to
convert as much as possible, but it will ignore certain parts that are not
applicable to modern Visual Studio projects. It will ignore any options that are
already defaults, reducing how much extra the files contain. It makes very few
sanity checks on the incoming data and because it only converts the project
file, it makes no guarantee that the output project file is going to build.

Note that this application does not handle the other portion of a project: the
workspace/solution file. If there becomes demand for it, it could be handled,
but for the time being, converting `.dsw` workspace files to `.sln` solution
files is not done.

## How to use it?

This application is a .NET 7.0 C# program. If you want to build it from the
source code, you will need to use Visual Studio 2022 and make sure you have the
[.NET 7.0 SDK][.NET 7.0] installed. If you just want to run the pre-built
binary, you will need the [.NET 7.0 runtime][.NET 7.0].

The application is a simple command line program. It takes a single argument,
the path to the `.dsp` project file you wish to convert. The resulting
`.vcxproj` project file is placed in the same directory as the `.dsp` file. You
can either run it from the command line (ideal in case there is an error) or you
can just drag the `.dsp` file onto the program.

## What to do if it doesn't work?

Testing this application is hard, mostly because finding projects that still
have only `.dsp` project files and not `.vcproj` or `.vcxproj` project files is
hard. I've tested it on a handful of `.dsp` project files that I had and used
that to try to fix any bugs, but there is always a possibility that the program
will encounter something it fails on.

To try to prevent failures, I've utilized the old MSDN documentation for Visual
Studio 6.0, the current Microsoft Learn documentation for Visual Studio (which
covers, 2015, 2017, 2019 and 2022, as of this writing in August of 2023), as
well as the help output of the various programs from both Visual Studio 6.0 and
Visual Studio 2022. In some cases, there has been conflicting documentation,
such as the help output not listing a flag that the MSDN documentation lists, or
one of them listing a flag that the other doesn't, etc.

In case you encounter a `.dsp` project file that does fail, please let me know
about it so I can handle it. You can submit a bug report in one of the following
ways:
* Submit an issue via
  [GitHub's issue tracker](https://github.com/CyberBotX/DSPtoVCXPROJ/issues)
* Contact me on Discord: username `cyberbotx`
* Email me: [cyberbotx@cyberbotx.com](mailto:cyberbotx@cyberbotx.com), please
  use the subject line `DSPtoVCXPROJ`.

## License

```
The MIT License (MIT)

Copyright (c) 2023 Naram "CyberBotX" Qashat

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
