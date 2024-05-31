#nullable enable

using System.ComponentModel.Composition;
using System.Threading;
using ChinesePinyinIntelliSenseExtender.Options;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.PatternMatching;
using Microsoft.VisualStudio.Utilities;

namespace ChinesePinyinIntelliSenseExtender.Intellisense.AsyncCompletion;

[Export(typeof(IAsyncCompletionItemManagerProvider))]
[Name(nameof(IdeographAsyncCompletionItemManagerProvider))]
[ContentType("text")]
[ContentType("Roslyn Languages")]
[Order(Before = PredefinedCompletionNames.DefaultCompletionItemManager)]
[TextViewRole(PredefinedTextViewRoles.Editable)]
[method: ImportingConstructor]
internal class IdeographAsyncCompletionItemManagerProvider(IPatternMatcherFactory patternMatcherFactory) : IAsyncCompletionItemManagerProvider
{
    #region Private 字段

    /// <summary>
    /// 递归标记
    /// </summary>
    private static readonly AsyncLocal<bool> s_completionItemManagerProviderRecursionTag = new();

    private static IdeographAsyncCompletionItemManager? s_instance;

    [ImportMany]
    private readonly Lazy<IAsyncCompletionItemManagerProvider>[] _lazyIAsyncCompletionItemManagerProviders = null!;

    private readonly IPatternMatcherFactory _patternMatcherFactory = patternMatcherFactory ?? throw new ArgumentNullException(nameof(patternMatcherFactory));

    #endregion Private 字段

    #region Public 方法

    public IAsyncCompletionItemManager GetOrCreate(ITextView textView)
    {
        if (s_completionItemManagerProviderRecursionTag.Value)
        {
            return null!;
        }

        var options = GeneralOptions.Instance;

        //非实验模式不使用 IdeographAsyncCompletionItemManager
        if (!options.Enable
            || options.AsyncCompletionMode != AsyncCompletionMode.Experimental)
        {
            try
            {
                s_completionItemManagerProviderRecursionTag.Value = true;

                foreach (var provider in _lazyIAsyncCompletionItemManagerProviders)
                {
                    if (provider.Value.GetOrCreate(textView) is IAsyncCompletionItemManager asyncCompletionItemManager)
                    {
                        return asyncCompletionItemManager;
                    }
                }
                //返回null应该是没问题的
                return null!;
            }
            finally
            {
                s_completionItemManagerProviderRecursionTag.Value = false;
            }
        }

        return s_instance ??= new IdeographAsyncCompletionItemManager(options, _patternMatcherFactory);
    }

    #endregion Public 方法
}
