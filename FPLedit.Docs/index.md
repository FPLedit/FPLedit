# FPLedit developer documentation

This is the developer documentation for the FPLedit project. FPLedit is a software to generate timetables for (model) railroads. For more information, refer to the official (German) website: [FPLedit Dokumentation & Download](https://fahrplan.manuelhu.de/).

## Scope of this documentation

The documents on this website describe how to build your own Extensions and Templates for the current version of the FPLedit application. Older versions are not available.

> [!NOTE]
> The normal user documentation of FPLedit is NOT available here, but on the dedicated website (and currently only in German): [FPLedit Dokumentation & Download](https://fahrplan.manuelhu.de/)

It is possible to extend FPLedit in two ways:

1. **Templates**: This is the easier to customize the output of one of the core modules of FPLedit that generate HTML output (in German: Buchfahrplan, Kursbuch, Aushangfahrplan)

   Templates are written in JavaScript (embedded in a T4-like template syntax) and have access to the object model. To create a template, you just need a text editor (preferably with syntax highlighting).
   
2. **Extensions**: FPLedit is extensible by design, and most of the functionality (even the core editor!) are in fact extensions.If you want something more fancy than "just" customizing the output, you can write your own.

   Extensions can be developed using any .NET language, like C#. Any IDE supporting the language will be supported, like MonoDevelop (free), Visual Studio Code (free), Visual Studio (free/paid) and Rider (paid).

If you built something working and cool, you can contact the author and maybe it will be included in the default build. If you have any questions, feel free to contact me, too.

> [!NOTE]
> Currently only the api documentation has been transferred here. All other developer documentation can still be found [on the official Website (German)](https://fahrplan.manuelhu.de/dev).
