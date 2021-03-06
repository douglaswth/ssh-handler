# SSH Handler

[![Release](https://img.shields.io/github/release/douglaswth/ssh-handler.svg?style=flat-square)][release]
[![Issues](https://img.shields.io/github/issues/douglaswth/ssh-handler.svg?style=flat-square)][issues]
[![Build](https://img.shields.io/appveyor/ci/douglaswth/ssh-handler/master.svg?style=flat-square)][build]

[release]: https://github.com/douglaswth/ssh-handler/releases/latest
[issues]: https://github.com/douglaswth/ssh-handler/issues
[build]: https://ci.appveyor.com/project/douglaswth/ssh-handler/branch/master

SSH Handler is a program that handles SSH URIs like `ssh://user@example.com` on
Windows. When you click an SSH link in your browser, it will launch [PuTTY] with
the correct SSH parameters.

[PuTTY]: http://www.chiark.greenend.org.uk/~sgtatham/putty/

## Installer

The installer will install SSH Handler and the .NET Framework 4 Client Profile
if it is needed:

* [ssh-handler-1.0.0.exe] `867 kB` (or [mirrored at dl.douglasthrift.net])

[ssh-handler-1.0.0.exe]: https://github.com/douglaswth/ssh-handler/releases/1.0.0/366/ssh-handler-1.0.0.exe
[mirrored at dl.douglasthrift.net]: https://dl.douglasthrift.net/ssh-handler/ssh-handler-1.0.0.exe

[PuTTY] must be installed from its installer or available in a directory in the
`PATH` environment variable.

## Building

The requirements for building SSH Handler and its installer are:

* [Visual Studio Express 2013 for Windows Desktop] or newer
* [NSIS (Nullsoft Scriptable Install System)]
* [Microsoft .NET Framework 4 Client Profile (Web Installer)]
* [NuGet Package Manager for Visual Studio 2013] included with newer versions of
  Visual Studio

[Visual Studio Express 2013 for Windows Desktop]: http://www.visualstudio.com/downloads/download-visual-studio-vs#d-express-windows-desktop
[NSIS (Nullsoft Scriptable Install System)]: http://nsis.sourceforge.net/
[Microsoft .NET Framework 4 Client Profile (Web Installer)]: http://www.microsoft.com/en-us/download/details.aspx?id=17113
[NuGet Package Manager for Visual Studio 2013]: http://visualstudiogallery.msdn.microsoft.com/4ec1526c-4a8c-4a84-b702-b21a8f5293ca

## License

    Copyright 2014 Douglas Thrift

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
