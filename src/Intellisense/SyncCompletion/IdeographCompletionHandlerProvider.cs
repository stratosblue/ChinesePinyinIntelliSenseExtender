using System.ComponentModel.Composition;

using ChinesePinyinIntelliSenseExtender.Options;

using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace ChinesePinyinIntelliSenseExtender.Intellisense.SyncCompletion;

[Export(typeof(IVsTextViewCreationListener))]
[Name("表意文字完成处理器")]
[ContentType("text")]
[TextViewRole(PredefinedTextViewRoles.Editable)]
internal class IdeographCompletionHandlerProvider : IVsTextViewCreationListener
{
    #region Internal 属性

    [Import]
    internal IVsEditorAdaptersFactoryService AdapterService { get; set; }

    [Import]
    internal ICompletionBroker CompletionBroker { get; set; }

    [Import]
    internal SVsServiceProvider ServiceProvider { get; set; }

    #endregion Internal 属性

    #region Public 方法

    public void VsTextViewCreated(IVsTextView textViewAdapter)
    {
        var textView = AdapterService.GetWpfTextView(textViewAdapter);
        if (textView == null)
        {
            return;
        }

        var options = GeneralOptions.Instance;

        if (options.Enable
            && options.EnableSyncCompletionSupport)
        {
            //RoslynLanguages 都为异步完成
            //此处可以进一步缓存与完善异步完成的类型
            //前置检查，理论上 ContentType 不会在编辑过程中改变？
            var isRoslynLanguagesBase = textView.TextBuffer.ContentType.BaseTypes.Any(i => i.TypeName == "Roslyn Languages");
            if (!isRoslynLanguagesBase) //异步完成不需要此逻辑，跳过 CommandHandler 处理
            {
                textView.Properties.GetOrCreateSingletonProperty(() => new IdeographCompletionCommandHandler(textViewAdapter, textView, CompletionBroker, ServiceProvider, options));
            }
        }
    }

    #endregion Public 方法
}
