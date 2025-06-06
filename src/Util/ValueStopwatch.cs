using System.Diagnostics;

namespace ChinesePinyinIntelliSenseExtender.Util;

internal struct ValueStopwatch(long stamp)
{
    #region Public 属性

    public readonly TimeSpan Elapsed
    {
        get
        {
            var curr = Stopwatch.GetTimestamp();
            var delta = curr - stamp;
            return new TimeSpan(delta * TimeSpan.TicksPerSecond / Stopwatch.Frequency);
        }
    }

    #endregion Public 属性

    #region Public 方法

    public static ValueStopwatch StartNew()
    {
        return new ValueStopwatch(Stopwatch.GetTimestamp());
    }

    #endregion Public 方法
}
