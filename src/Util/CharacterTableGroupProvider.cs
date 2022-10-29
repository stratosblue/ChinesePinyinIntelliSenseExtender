#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ChinesePinyinIntelliSenseExtender.Options;

namespace ChinesePinyinIntelliSenseExtender.Util;

internal static class CharacterTableGroupProvider
{
    #region Private 字段

    private static TaskCompletionSource<CharacterTableGroup>? s_completionSource;

    private static string[]? s_lastDicPaths;

    #endregion Private 字段

    #region Public 方法

    public static Task<CharacterTableGroup> ChangeAsync(IEnumerable<string> dicPaths, CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref s_lastDicPaths, dicPaths.ToArray()) is string[] lastDicPaths
            && !(dicPaths.Except(lastDicPaths).Any() || lastDicPaths.Except(dicPaths).Any())
            && s_completionSource is TaskCompletionSource<CharacterTableGroup> lastCompletionSource)
        {
            return lastCompletionSource.Task;
        }

        var completionSource = new TaskCompletionSource<CharacterTableGroup>();

        if (Interlocked.Exchange(ref s_completionSource, completionSource) is TaskCompletionSource<CharacterTableGroup> oldCompletionSource)
        {
            oldCompletionSource.TrySetCanceled();
        }

        if (dicPaths.Any())
        {
            _ = Task.Factory.StartNew(async _ =>
            {
                try
                {
                    var tasks = dicPaths.Select(m => CharacterTableLoader.LoadFileAsync(m, cancellationToken)).ToArray();

                    await Task.WhenAll(tasks);

                    var group = new CharacterTableGroup(tasks.Select(m => m.Result).ToList());
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
            completionSource.TrySetResult(new CharacterTableGroup(new[] { new CharacterTable(new()) }));
        }

        return completionSource.Task;
    }

    public static Task<CharacterTableGroup> GetAsync() => s_completionSource is null ? throw new InvalidOperationException("Not init yet.") : s_completionSource.Task;

    public static Task<CharacterTableGroup> LoadFromOptionsAsync(GeneralOptions options, CancellationToken cancellationToken)
    {
        var dicPaths = new List<string>();
        if (options.EnableBuiltInWubi86Dic)
        {
            dicPaths.Add(CharacterTableLoader.Wubi86DicPath);
        }
        if (options.EnableBuiltInPinyinDic)
        {
            dicPaths.Add(CharacterTableLoader.PinyinDicPath);
        }

        if (options.CustomAdditionalDictionaryPaths?.Length > 0)
        {
            dicPaths.AddRange(options.CustomAdditionalDictionaryPaths.Select(m => m.Trim()));
        }

        return ChangeAsync(dicPaths, cancellationToken);
    }

    #endregion Public 方法
}
