# FPLedit - Timetable generator

FPLedit is an application to generate model railroad timetables in different output formats.

Currently the application is mostly only available in German, and the provided templates are mostly similar to German timetables used by the former DB and DR. Some English translations of the application UI exist. The translations are created in the `gettext` format and also kept inside this repository.

The documentation (German only), release download and example files are available at [the official website](https://fahrplan.manuelhu.de/). API documentation built from this repo is available at https://apidoc.fpledit.de.

## Building FPLedit

FPLedit currently uses the following dependencies from NuGet:

* Eto.Forms - UI Framework
* Jint - Javascript interpreter - used for templating
* DeepCloner - used to generate checkpoints for undo operations
* NGettext - used to localize the user interface
* various .NET Core dependencies.
* ... and some more dependencies that are only used to build / test the application (Cake, docfx, ...).

Some of those packages are taken from different repositories. The `NuGet.config` in the repository root should take care of that.

FPLedit currently can only be built and run with .NET 6. Support for .NET 5 and .NET 4.x/mono has been discontinued. .NET 7 is also not supported at the moment, as FPLedit still heavily uses `System.Drawing.Common` on Unix-like platforms.

## Packaging a release

To build a relaease run `dotnet cake` in the repository root. Before you run cake the first time, you have to run `dotnet tool restore`.

Accepted flags are `--auto-beta` which will build and assign a -betaX version number. Target plaforms for cross-platform builds are specified with `--rid=rid1,rid2,...`. Available rids are `linux-x64`, `osx-x64`, `win-x64` and `win-x64`. All other options are mainly specified with environment variables.

Bundled documetation:
* If the `FPLEDIT_DOK_REPO` env variable is set to the folder of the documentation repo, the packaging scripts will also build the FPLedit offline pdf documentation. This requires an installation of `xelatex`, `sass`, `pandoc` and [hugo](https://gohugo.io) in your path.
* If the `FPLEDIT_DOK_PDF` env variable is set to a (full) path of a pdf file, this will be copied to the distribution zip files as `Dokumentation.pdf`.

Git snapshots:
* `FPLEDIT_GIT`: This env variable should be set to the full commit sha1 hash (if applicable)
* `FPLEDIT_GIT_BETA`: Toggles the development version mode (e.g. for CI runs)
