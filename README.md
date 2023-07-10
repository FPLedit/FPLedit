# FPLedit - Railway Timetable Generator

FPLedit (FahrPLan EDITor) is an application to generate model railway timetables in different output formats.

Currently the application is mostly only available in German, and the provided templates are mostly similar to German timetables used by the former DB and DR. Some English translations of the application UI exist. The translations are created in the `gettext` format and also kept inside this repository.

The documentation (German only), release download and example files are available at [the official website](https://fahrplan.manuelhu.de/). API documentation built from this repo is available at https://apidoc.fpledit.de.

## Building FPLedit

FPLedit currently uses the following dependencies from NuGet:

* Eto.Forms - UI Framework (using Wpf on Windows, Gtk3 on Linux, ...)
* Jint - Javascript interpreter (used for templating)
* DeepCloner - used to generate checkpoints for undo operations
* NGettext - used to localize the user interface
* ImageSharp, ImageSharp.Drawing, SixLabors.Fonts and PdfSharp - drawing of graphical timetables.
* various .NET Core dependencies.
* ... and some more dependencies that are only used to build / test the application (Cake, docfx, ...).

## Supported .NET Versions

FPLedit currently can only be built and run with .NET 6. Support for .NET 5 and .NET 4.x/mono has been discontinued.

.NET 7 is also not yet *officially* supported at the moment, but planned/in preparation.

## Packaging a release

To build a relaease run `dotnet cake` in the repository root. Before you run cake the first time, you have to run `dotnet tool restore`.

Accepted flags are `--auto-beta` which will build and assign a -betaX version number. Target plaforms for cross-platform builds are specified with `--rid=rid1,rid2,...`. Available rids are `linux-x64`, `osx-x64`, `win-x64` and `win-x86`. All other options are mainly specified with environment variables:

* `FPLEDIT_DOK_PDF`: If this env variable is set to a (full) path of a pdf file, it will be copied to the distribution zip files as `Dokumentation.pdf`. By default, the file in the root of this repository will be used.
* `FPLEDIT_GIT`: This env variable should be set to the full commit sha1 hash (if applicable; to produce a git snapshot)
* `FPLEDIT_GIT_BETA`: Toggles the development version mode (e.g. for creation of git snapshots in CI runs)
