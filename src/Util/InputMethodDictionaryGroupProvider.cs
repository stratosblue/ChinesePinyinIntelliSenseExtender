#nullable enable

using System.Threading;

using ChinesePinyinIntelliSenseExtender.Options;

using InputMethodDictionary;

namespace ChinesePinyinIntelliSenseExtender.Util;

internal class InputMethodDictionaryGroupProvider
{
    #region Private 字段

    private static TaskCompletionSource<InputMethodDictionaryGroup>? s_completionSource;

    private static string[]? s_lastDicPaths;

    #endregion Private 字段

    #region Public 方法

    public static Task<InputMethodDictionaryGroup> ChangeAsync(IEnumerable<string> dicPaths, CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref s_lastDicPaths, dicPaths.ToArray()) is string[] lastDicPaths
            && !(dicPaths.Except(lastDicPaths).Any() || lastDicPaths.Except(dicPaths).Any())
            && s_completionSource is TaskCompletionSource<InputMethodDictionaryGroup> lastCompletionSource)
        {
            return lastCompletionSource.Task;
        }

        var completionSource = new TaskCompletionSource<InputMethodDictionaryGroup>();

        if (Interlocked.Exchange(ref s_completionSource, completionSource) is TaskCompletionSource<InputMethodDictionaryGroup> oldCompletionSource)
        {
            oldCompletionSource.TrySetCanceled();

            try
            {
                if (oldCompletionSource.Task.Status == TaskStatus.RanToCompletion)
                {
                    var oldGroup = oldCompletionSource.Task.Result;
                    oldGroup.Dispose();
                }
            }
            catch { }
        }

        if (dicPaths.Any())
        {
            _ = Task.Factory.StartNew(async _ =>
            {
                try
                {
                    var tasks = dicPaths.Select(m => InputMethodDictionaryLoader.LoadFileAsync(m, cancellationToken)).ToArray();

                    await Task.WhenAll(tasks);

                    var group = new InputMethodDictionaryGroup(tasks.Select(m => m.Result).ToList());
                    completionSource.TrySetResult(group);
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    completionSource.TrySetException(ex);
                    throw;
                }
            }, null, cancellationToken, TaskCreationOptions.RunContinuationsAsynchronously, TaskScheduler.Default)
            .ContinueWith(_ => completionSource.TrySetCanceled(), CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled, TaskScheduler.Default);
        }
        else
        {
            completionSource.TrySetResult(new InputMethodDictionaryGroup(new[] { new InputMethodReverseDictionary(string.Empty.AsMemory()) }));
        }

        return completionSource.Task;
    }

    public static Task<InputMethodDictionaryGroup> GetAsync() => s_completionSource is null ? throw new InvalidOperationException("Not init yet.") : s_completionSource.Task;

    public static Task<InputMethodDictionaryGroup> LoadFromOptionsAsync(GeneralOptions options, CancellationToken cancellationToken)
    {
        var dicPaths = new List<string>();
        if (options.EnableBuiltInWubi86Dic)
        {
            dicPaths.Add(InputMethodDictionaryLoader.Wubi86DicPath);
        }
        if (options.EnableBuiltInPinyinDic)
        {
            dicPaths.Add(InputMethodDictionaryLoader.PinyinDicPath);
        }

        if (options.CustomAdditionalDictionaryPaths?.Length > 0)
        {
            dicPaths.AddRange(options.CustomAdditionalDictionaryPaths.Select(m => m.Trim()));
        }

        return ChangeAsync(dicPaths, cancellationToken);
    }

    #endregion Public 方法
}
