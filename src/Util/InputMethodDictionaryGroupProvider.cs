using ChinesePinyinIntelliSenseExtender.Options;

using InputMethodDictionary;

namespace ChinesePinyinIntelliSenseExtender.Util;

internal class InputMethodDictionaryGroupProvider
{
    #region Private 字段

    private static TaskCompletionSource<InputMethodDictionaryGroup>? s_completionSource;

    private static DictionaryCombination[]? s_lastDictionaryCombinations;

    #endregion Private 字段

    #region Public 方法

    public static Task<InputMethodDictionaryGroup> ChangeAsync(IEnumerable<DictionaryCombination> combinations, CancellationToken cancellationToken)
    {
        var dictionaryCombinations = combinations.Distinct().OrderBy(m => m.GetHashCode()).ToArray();
        if (Interlocked.Exchange(ref s_lastDictionaryCombinations, dictionaryCombinations) is DictionaryCombination[] lastDictionaryCombinations
            && !(dictionaryCombinations.Except(lastDictionaryCombinations).Any() || lastDictionaryCombinations.Except(dictionaryCombinations).Any())
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
#pragma warning disable VSTHRD103 // Call async methods when in an async method
                    var oldGroup = oldCompletionSource.Task.Result;
#pragma warning restore VSTHRD103 // Call async methods when in an async method
                    oldGroup.Dispose();
                }
            }
            catch { }
        }

        if (dictionaryCombinations.Any())
        {
            _ = Task.Factory.StartNew(async _ =>
            {
                try
                {
                    var tasks = dictionaryCombinations.Select(m => InputMethodDictionaryLoader.LoadFilesAsync(m.OrderedDictionaries.Select(m => m.FilePath), cancellationToken)).ToArray();

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

    public static Task<InputMethodDictionaryGroup> LoadFromOptionsAsync(DictionaryManageOptions options, CancellationToken cancellationToken)
    {
        return ChangeAsync(options.DictionaryCombinations, cancellationToken);
    }

    #endregion Public 方法
}
