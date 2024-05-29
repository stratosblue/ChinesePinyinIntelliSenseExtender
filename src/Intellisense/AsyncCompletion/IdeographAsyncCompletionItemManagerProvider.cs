#nullable enable

using System.ComponentModel.Composition;
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
internal class IdeographAsyncCompletionItemManagerProvider : IAsyncCompletionItemManagerProvider
{
    [Import]
    public IPatternMatcherFactory PatternMatcherFactory = null!;

    private static IdeographAsyncCompletionItemManager? s_instance;

    public IdeographAsyncCompletionItemManagerProvider()
    {
    }

    public IAsyncCompletionItemManager GetOrCreate(ITextView textView)
    {
        return s_instance ??= new IdeographAsyncCompletionItemManager(PatternMatcherFactory);
    }
}
