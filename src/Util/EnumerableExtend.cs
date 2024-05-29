#nullable enable

using System.Threading;

namespace ChinesePinyinIntelliSenseExtender.Util;

internal static class EnumerableExtend
{
    public static U[] ParallelChoose<T, U>(this IReadOnlyList<T> array, Func<T, (bool, U)> chooser)
    {
        var inputLength = array.Count;
        var isChosen = new bool[inputLength];
        var result = new U[inputLength];
        var outputLength = 0;

        Parallel.For(0, inputLength, () => 0, (i, _, count) =>
        {
            var (a, b) = chooser(array[i]);
            if (a)
            {
                isChosen[i] = true;
                result[i] = b;
                count++;
            }
            return count;
        }, x => Interlocked.Add(ref outputLength, x));

        var output = new U[outputLength];
        var curr = 0;
        for (int i = 0; i < inputLength; i++)
        {
            if (isChosen[i]) output[curr++] = result[i];
        }
        return output;
    }
}
