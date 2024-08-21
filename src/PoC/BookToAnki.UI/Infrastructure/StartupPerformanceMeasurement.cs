using System;
using System.Diagnostics;

namespace BookToAnki.UI.Infrastructure;
internal static class StartupPerformanceMeasurement
{
    static Stopwatch? stopwatch = null;
    internal static void StartMeasuring()
    {
        stopwatch = Stopwatch.StartNew();
    }
    internal static TimeSpan Stop()
    {
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }
}
