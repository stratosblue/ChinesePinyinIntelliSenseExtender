#nullable enable
#pragma warning disable VSTHRD010

using System.Runtime.InteropServices;
using ChinesePinyinIntelliSenseExtender.Options;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace ChinesePinyinIntelliSenseExtender.Intellisense.SyncCompletion;

internal class IdeographCompletionCommandHandler : IOleCommandTarget
{
    #region Private 字段

    private readonly ICompletionBroker _completionBroker;

    private readonly IOleCommandTarget _nextCommandHandler;

    private readonly GeneralOptions _options;

    private readonly SVsServiceProvider _serviceProvider;

    private readonly ITextView _textView;

    #endregion Private 字段

    #region Internal 构造函数

    internal IdeographCompletionCommandHandler(IVsTextView textViewAdapter, ITextView textView, ICompletionBroker completionBroker, SVsServiceProvider serviceProvider, GeneralOptions options)
    {
        _textView = textView;
        _completionBroker = completionBroker;
        _serviceProvider = serviceProvider;
        _options = options;

        textViewAdapter.AddCommandFilter(this, out _nextCommandHandler);
    }

    #endregion Internal 构造函数

    #region Public 方法

    public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
    {
        //see https://learn.microsoft.com/zh-cn/visualstudio/extensibility/walkthrough-displaying-statement-completion?view=vs-2022&tabs=csharp#tabpanel_19_csharp

        if (VsShellUtilities.IsInAutomationFunction(_serviceProvider)
            || !_options.EnableSyncCompletionSupport)
        {
            return _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        var isTypedWhiteSpace = false;
        var isTypedPunctuation = false;

        if (pguidCmdGroup == VSConstants.VSStd2K
            && nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
        {
            var typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);

            if (char.IsWhiteSpace(typedChar))
            {
                isTypedWhiteSpace = true;
            }
            else
            {
                isTypedPunctuation = char.IsPunctuation(typedChar);
            }
        }

        if (nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN
            || nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB
            || isTypedWhiteSpace)
        {
            var completionSessions = _completionBroker.GetSessions(_textView);
            foreach (var completionSession in completionSessions)
            {
                if (!completionSession.IsDismissed)
                {
                    if (completionSession.SelectedCompletionSet.SelectionStatus.IsSelected)
                    {
                        // 暂时调整了匹配逻辑，至少int可以正常输入了，开启空格输入
                        //if (isTypedWhiteSpace) //暂时取消空格输入，避免类似：输入 int 后键入空格，会插入模糊匹配的完成项
                        //{
                        //    completionSession.Dismiss();
                        //}
                        //else
                        //{
                        //    completionSession.Commit();
                        //}

                        completionSession.Commit();

                        if (isTypedWhiteSpace || isTypedPunctuation)
                        {
                            _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                        }
                        return VSConstants.S_OK;
                    }
                    else
                    {
                        completionSession.Dismiss();
                    }
                }
            }
        }

        return _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
    }

    public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) => _nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

    #endregion Public 方法
}
