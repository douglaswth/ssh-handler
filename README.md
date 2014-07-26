# SSH Handler

[![Release](https://img.shields.io/github/release/douglaswth/ssh-handler.svg?style=flat)][release]
[![Issues](https://img.shields.io/github/issues/douglaswth/ssh-handler.svg?style=flat)][issues]

[release]: https://github.com/douglaswth/ssh-handler/releases/latest
[issues]: https://github.com/douglaswth/ssh-handler/issues

SSH Handler is a program that handles SSH URIs like [ssh://user@example.com]
(ssh://user@example.com) on Windows. When you click an SSH link in your browser,
it will launch [PuTTY](http://www.chiark.greenend.org.uk/~sgtatham/putty/) with
the correct SSH parameters.

## Installer

The installer will install SSH Handler and the .NET Framework 4 Client Profile
if it is needed:

* [ssh-handler-1.0.0.exe]
(https://github.com/douglaswth/ssh-handler/releases/1.0.0/366/ssh-handler-1.0.0.exe) `867 kB`

[PuTTY](http://www.chiark.greenend.org.uk/~sgtatham/putty/) must be installed
from its installer or available in directory in the `PATH` environment variable.

## Building

The requirements for building SSH Handler and its installer are:

* [Visual Studio Express 2013 for Windows Desktop]
(http://www.visualstudio.com/downloads/download-visual-studio-vs#d-express-windows-desktop)
* [NSIS (Nullsoft Scriptable Install System)](http://nsis.sourceforge.net/)
* [Microsoft .NET Framework 4 Client Profile (Web Installer)]
(http://www.microsoft.com/en-us/download/details.aspx?id=17113)

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
