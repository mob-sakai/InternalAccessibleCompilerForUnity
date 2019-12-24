# NoAccessibilityCompiler

`NoAccessibilityCompiler` is a Roslyn compiler **without accessibility check**.

![icon](https://user-images.githubusercontent.com/12690315/69955042-1dc8a380-1540-11ea-9d38-fa7fa77b22d9.png)

[![Nuget](https://img.shields.io/nuget/v/NoAccessibilityCompiler)](https://www.nuget.org/packages/NoAccessibilityCompiler)
![GitHub](https://img.shields.io/github/license/mob-sakai/NoAccessibilityCompiler)
![Nuget](https://img.shields.io/nuget/dt/NoAccessibilityCompiler)
![release](https://github.com/mob-sakai/NoAccessibilityCompiler/workflows/Release/badge.svg)

## Summary

`NoAccessibilityCompiler` is a Roslyn compiler without accessibility check.

(Sample code)

## Installation

```bash
$ dotnet tool install --global NoAccessibilityCompiler
```

Or, download from [release page](https://github.com/mob-sakai/NoAccessibilityCompiler/releases)

## Usage

```sh
NoAccessibilityCompiler --output your.dll your.csproj
  -o, --output            (Default: ) Output path｡ If it is empty, a dll is generated in the same path as csproj.
  -a, --assemblyNames     (Default: ) Target assembly names separated by semicolons to access internally
  -c, --configuration     (Default: Release) Configuration
  -l, --logfile           (Default: compile.log) Logfile path
  --help                  Display this help screen.
  --version               Display version information.
  ProjectPath (pos. 1)    Input .csproj path
```

## License

MIT

## See Also

- GitHub page : https://github.com/mob-sakai/NoAccessibilityCompiler
- Nuget page : https://www.nuget.org/packages/NoAccessibilityCompiler
- For Unity version : https://www.nuget.org/packages/NoAccessibilityCompilerForUnity

[![become_a_sponsor_on_github](https://user-images.githubusercontent.com/12690315/66942881-03686280-f085-11e9-9586-fc0b6011029f.png)](https://github.com/users/mob-sakai/sponsorship)
