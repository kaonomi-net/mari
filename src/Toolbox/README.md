# MARI.Toolbox

Technical library shared by all the M-A-R-I projects.

> [!NOTE]
> This library was previously published as the separate packages **Elvex.Toolbox**.<br />
> It is now part of the M-A-R-I repository and renamed **MARI.Toolbox** (namespaces `Elvex.Toolbox.*` -> `MARI.Toolbox.*`).

## Projects

| Project | Content |
| --- | --- |
| MARI.Toolbox.Abstractions | Interfaces, models, attributes, conditions, expressions, ... |
| MARI.Toolbox | Implementations: extensions, serialization (Newtonsoft converters, type solver), disposables, patterns (strategy, workers), services (hash, file system, globalization), ... |
| MARI.Toolbox.Configuration | Configuration helpers |
| MARI.Toolbox.WPF / .WPF.Abstractions | WPF tools (Windows only) |
| MARI.Toolbox.WPF.UI / .WPF.UI.Avalon | WPF controls, themes and AvalonDock integration (Windows only), XAML namespace `http://mari.toolbox.ui/` |
| tests/MARI.Toolbox.UnitTests.ToolKit | Unit test helpers (xunit, AutoFixture, NFluent, NSubstitute) |
| tests/* | Unit tests and WPF / ComServer test applications |
| benchmarks/MARI.Toolbox.Benchmark.Sandbox | BenchmarkDotNet sandbox |

## Build and test

The projects are part of `MARI.sln` (solution folders *Toolbox*, *Toolbox/Tests* and *Toolbox/Benchmarks*):

```shell
dotnet build MARI.sln
dotnet test MARI.sln
```

## License

MIT, cf. [LICENSE](LICENSE) (original author: Mickael Thumerel).
