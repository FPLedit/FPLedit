using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FPLedit.Bildfahrplan.Render
{
    internal sealed class Profiler
    {
        private readonly Stopwatch stopWatch;
        private readonly string prefix;

        public Profiler(string prefix)
        {
#if DEBUG
            this.prefix = prefix;
            stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Profile(string text)
        {
#if DEBUG
            Trace.WriteLine($"[PROF|{prefix}] {text} {stopWatch.ElapsedMilliseconds}\n");
            stopWatch.Restart();
#endif
        }
    }
}
