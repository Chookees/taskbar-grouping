# Third-Party Notices

TaskbarFolders is distributed under the [MIT License](LICENSE). The released binaries are self-contained, so they include the .NET runtime and the packages below.

Licence identifiers are taken from each package's published metadata. Consult the linked project for the authoritative licence text.

## Included in the released binaries

| Component | Version | Licence |
|---|---|---|
| [.NET Runtime and Windows Desktop Runtime](https://github.com/dotnet/runtime) | 8.0 | MIT |
| [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) | 8.3.2 | MIT |
| [Microsoft.Extensions.DependencyInjection](https://github.com/dotnet/runtime) | 8.0.1 | MIT |
| [Microsoft.Extensions.Hosting](https://github.com/dotnet/runtime) | 8.0.1 | MIT |
| [Microsoft.Extensions.Logging](https://github.com/dotnet/runtime) | 8.0.1 | MIT |
| [Microsoft.Extensions.Logging.Abstractions](https://github.com/dotnet/runtime) | 8.0.2 | MIT |
| [Microsoft.Extensions.Options](https://github.com/dotnet/runtime) | 8.0.2 | MIT |
| [System.Text.Json](https://github.com/dotnet/runtime) | 8.0.5 | MIT |

Each of these carries the following notice:

```
Copyright (c) .NET Foundation and Contributors

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

## Build and test only — not distributed

These are used to build and test the project and are not part of any released artifact:

| Component | Version | Licence |
|---|---|---|
| [xUnit.net](https://github.com/xunit/xunit) | 2.9.2 | Apache-2.0 |
| [Microsoft.NET.Test.Sdk](https://github.com/microsoft/vstest) | 17.11.1 | MIT |
| [Moq](https://github.com/devlooped/moq) | 4.20.72 | BSD-3-Clause |
| [FluentAssertions](https://github.com/fluentassertions/fluentassertions) | 6.12.2 | Apache-2.0 |
| [coverlet.collector](https://github.com/coverlet-coverage/coverlet) | 6.0.2 | MIT |
| [ReportGenerator](https://github.com/danielpalme/ReportGenerator) | 5.4.4 | Apache-2.0 |
| [Inno Setup](https://jrsoftware.org/isinfo.php) | 6 | Inno Setup licence |

FluentAssertions is deliberately pinned below version 8; that release moved to a commercial licence. Dependabot is configured to ignore updates past 7.x.

## Windows APIs

The application calls Windows APIs directly through P/Invoke and COM — `SHGetFileInfo`, `IImageList`, `IShellLinkW`, `IPropertyStore`, `SHChangeNotify`, `SHAppBarMessage`, DWM window attributes — and the WinRT `Windows.UI.Shell.TaskbarManager` projection. These are part of Windows; no third-party redistributable is involved.

## Reporting an omission

If something is missing or misattributed here, please open an issue.
