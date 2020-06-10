namespace FPLedit.Config
{
    internal static class OptionsParser
    {
        public static string OpenFilename { get; internal set; }

        public static bool ConsoleLog { get; internal set; }

        public static bool TemplateDebug { get; internal set; }
        
        public static bool CrashReporterDebug { get; internal set; }
    }
}
