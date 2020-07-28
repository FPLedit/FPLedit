# FPLedit - Timetable generator

FPLedit is an application to generate model railroad timetables in different output formats.

Currently the application is only availbale in German, and the provided templates are mostly similar to German timetables used by the former DB and DR.

The documentation (German only), release download and example files are available at [the official website](https://fahrplan.manuelhu.de/).

## Building FPLedit

FPLedit currently uses the following dependencies from NuGet:

* Eto.Forms - UI Framework
* Jint - Javascript interpreter - used for templating
* DeepCloner - used to generate checkpoints for undo operations
* NGettext - used to localize the user interface
* various .NET Core dependencies.
* ... and some more dependencies that are only used to build / test the application.

Some of those packages are taken from different repositories. The `NuGet.config` in the repository root should take care of that.

To build FPLedit you *need* msbuild and not xbuild (on Linux you can install it from the mono repository). The .NET Core msbuild is *not* enough, you specifically need msbuild version 15.0 from mono and all the mono libraries.

FPLedit currently can't be built or run with .NET core!

## Packaging a release

To build a relaease run `build.sh` or `build.ps1` in the repository root.

Accepted flags are `--auto-beta` which will build and assign a -betaX version number.

If the `FPLEDIT_DOK` env variable is set to the folder of the documentation repo, the packaging scripts will also build the FPLedit offline pdf documentation. This requires an installation of `xelatex`, `sass` and [hugo](https://gohugo.io) in your path.

