using Microsoft.Win32;
using System;

namespace FPLedit.jTrainGraphStarter
{
    internal static class JavaFinder
    {
        public static string JavaGuess()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return "java"; // Not supported
            // Disable warnings "Registry.GetValue" is only supported on 'windows'".
#pragma warning disable CA1416

            string[] keys = 
            {
                @"HKEY_LOCAL_MACHINE\Software\JavaSoft\Java Runtime Environment",
                @"HKEY_LOCAL_MACHINE\Software\Wow6432Node\JavaSoft\Java Runtime Environment",
                @"HKEY_LOCAL_MACHINE\Software\JavaSoft\Java Development Kit",
                @"HKEY_LOCAL_MACHINE\Software\Wow6432Node\JavaSoft\Java",
            };
            
            foreach (var key in keys)
            {
                var x = (string)Registry.GetValue(key, "CurrentVersion", null);
                if (string.IsNullOrEmpty(x))
                    continue;

                var key2 = $@"{key}\{x}";
                var home = (string)Registry.GetValue(key2, "JavaHome", null);

                if (string.IsNullOrEmpty(home))
                    continue;
                return home + @"\bin\javaw.exe";
            }

            return null;
#pragma warning restore CA1416
        }
    }
}
