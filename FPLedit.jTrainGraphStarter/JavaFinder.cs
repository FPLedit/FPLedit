using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.jTrainGraphStarter
{
    internal class JavaFinder
    {
        public static string JavaGuess()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return "java"; // Not supported

            string[] keys = new string[]
            {
                @"HKEY_LOCAL_MACHINE\Software\JavaSoft\Java Runtime Environment",
                @"HKEY_LOCAL_MACHINE\Software\Wow6432Node\JavaSoft\Java Runtime Environment",
                @"HKEY_LOCAL_MACHINE\Software\JavaSoft\Java Development Kit",
                @"HKEY_LOCAL_MACHINE\Software\Wow6432Node\JavaSoft\Java",
            };

            foreach (var key in keys)
            {
                var x = (string)Registry.GetValue(key, "CurrentVersion", null);
                if (x == null || x == "")
                    continue;

                var key2 = $@"{key}\{x}";
                var home = (string)Registry.GetValue(key2, "JavaHome", null);

                if (home == null || home == "")
                    continue;
                return home + @"\bin\javaw.exe";
            }

            return null;
        }
    }
}
