#nullable enable

using System.Diagnostics;

namespace ChinesePinyinIntelliSenseExtender.Util;

internal struct ValueStopwatch(long stamp)
{
    public readonly TimeSpan Elapsed
    {
        get
        {
            var curr = Stopwatch.GetTimestamp();
            var delta = curr - stamp;
            return new TimeSpan(delta * TimeSpan.TicksPerSecond / Stopwatch.Frequency);
        }
    }

    public static ValueStopwatch StartNew()
    {
        return new ValueStopwatch(Stopwatch.GetTimestamp());
    }
}
