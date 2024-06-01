#nullable enable

using System.ComponentModel.Composition;
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

    private static IdeographAsyncCompletionItemManager? s_instance;

    private readonly IPatternMatcherFactory _patternMatcherFactory = patternMatcherFactory ?? throw new ArgumentNullException(nameof(patternMatcherFactory));

    #endregion Private 字段

    #region Public 方法

    public IAsyncCompletionItemManager? GetOrCreate(ITextView textView)
    {
        var options = GeneralOptions.Instance;

        //非实验模式不使用 IdeographAsyncCompletionItemManager
        if (!options.Enable
            || options.AsyncCompletionMode != AsyncCompletionMode.Experimental)
        {
            return null;
        }

        return s_instance ??= new IdeographAsyncCompletionItemManager(options, _patternMatcherFactory);
    }

    #endregion Public 方法
}
