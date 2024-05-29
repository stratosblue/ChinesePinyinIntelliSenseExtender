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

//[Export(typeof(IVsTextViewCreationListener))]
//[ContentType("text")]
//[TextViewRole(PredefinedTextViewRoles.Editable)]
//internal sealed class VsTextViewCreationListener : IVsTextViewCreationListener
//{
//    [Import]
//    private readonly IVsEditorAdaptersFactoryService _adaptersFactory = null!;

//    [Import]
//    private readonly ICompletionBroker _completionBroker = null!;

//    public async void VsTextViewCreated(IVsTextView textViewAdapter)
//    {
//        var textView = _adaptersFactory.GetWpfTextView(textViewAdapter);
//        VMCommandFilter filter = new VMCommandFilter(textView!, _completionBroker, await GetInputMethodDictionaryGroupAsync());
//        textViewAdapter.AddCommandFilter(filter, out IOleCommandTarget next);
//        filter.NextCommandHandler = next;
//    }
//    private InputMethodDictionaryGroup? _inputMethodDictionaryGroup;
//    protected async Task<InputMethodDictionaryGroup> GetInputMethodDictionaryGroupAsync()
//    {
//        if (_inputMethodDictionaryGroup is null
//            || _inputMethodDictionaryGroup.IsDisposed)
//        {
//            _inputMethodDictionaryGroup = await InputMethodDictionaryGroupProvider.GetAsync();
//        }
//        return _inputMethodDictionaryGroup;
//    }
//}

//internal class VMCommandFilter : IOleCommandTarget
//{
//    /// <summary>
//    /// 当前会话
//    /// </summary>
//    ICompletionSession _currentSession;
//    private readonly InputMethodDictionaryGroup _inputMethodDictionaryGroup;

//    /// <summary>
//    /// TextView(WPF)
//    /// </summary>
//    IWpfTextView TextView { get; set; }

//    /// <summary>
//    /// 代理
//    /// </summary>
//    ICompletionBroker Broker { get; set; }

//    /// <summary>
//    /// 执行由VMCommandFilter未执行完的命令
//    /// </summary>
//    public IOleCommandTarget NextCommandHandler { get; set; }

//    public VMCommandFilter(IWpfTextView textView, ICompletionBroker broker, InputMethodDictionaryGroup inputMethodDictionaryGroup)
//    {
//        this.TextView = textView;
//        this.Broker = broker;
//        _inputMethodDictionaryGroup = inputMethodDictionaryGroup;
//    }
//    /// <summary>
//    /// 获取输入的字符
//    /// </summary>
//    /// <param name="pvaIn">输入指针</param>
//    private char GetTypeChar(IntPtr pvaIn)
//    {
//        return (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
//    }

//    /// <summary>
//    /// 执行指定的命令
//    /// </summary>
//    /// <param name="pguidCmdGroup">命令组的 GUID</param>
//    /// <param name="nCmdID">命令 ID</param>
//    /// <param name="nCmdexecopt">指定对象应如何执行命令</param>
//    /// <param name="pvaIn">命令的输入参数</param>
//    /// <param name="pvaOut">命令的输出参数</param>
//    public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
//    {

//        ThreadHelper.ThrowIfNotOnUIThread();

//        //AsmDudeToolsStatic.Output_INFO(string.Format(AsmDudeToolsStatic.CultureUI, "{0}:ExecMethod2", this.ToString()));

//        bool handledChar = false;
//        int hresult = VSConstants.S_OK;
//        char typedChar = char.MinValue;

//        #region 1. Pre-process
//        if (pguidCmdGroup == VSConstants.VSStd2K)
//        {
//            switch ((VSConstants.VSStd2KCmdID)nCmdID)
//            {
//                case VSConstants.VSStd2KCmdID.AUTOCOMPLETE:
//                case VSConstants.VSStd2KCmdID.COMPLETEWORD:
//                    handledChar = this.StartSession();
//                    break;
//                case VSConstants.VSStd2KCmdID.RETURN:
//                    handledChar = this.Complete(true);
//                    break;
//                case VSConstants.VSStd2KCmdID.TAB:
//                    this.Complete(true);
//                    handledChar = false;
//                    break;
//                case VSConstants.VSStd2KCmdID.CANCEL:
//                    handledChar = this.Cancel();
//                    break;
//                case VSConstants.VSStd2KCmdID.TYPECHAR:
//                    typedChar = GetTypeChar(pvaIn);
//                    if (char.IsWhiteSpace(typedChar))
//                    {
//                        this.Complete(true);
//                        handledChar = false;
//                    }
//                    break;
//            }
//        }
//        #endregion

//        #region 2. Handle the typed char
//        if (!handledChar)
//        {
//            hresult = this.NextCommandHandler.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

//            if (!typedChar.Equals(char.MinValue) && char.IsLetterOrDigit(typedChar))
//            {
//                //if (!typedChar.Equals(char.MinValue)) {
//                if ((this._currentSession == null) || this._currentSession.IsDismissed)
//                { // If there is no active session, bring up completion
//                    this.StartSession();
//                }
//                this.Filter();
//                hresult = VSConstants.S_OK;
//            }
//            else if (nCmdID == (uint)VSConstants.VSStd2KCmdID.BACKSPACE //redo the filter if there is a deletion
//                  || nCmdID == (uint)VSConstants.VSStd2KCmdID.DELETE)
//            {
//                if ((this._currentSession != null) && !this._currentSession.IsDismissed)
//                {
//                    this.Filter();
//                }
//                hresult = VSConstants.S_OK;
//            }
//        }
//        #endregion

//        #region Post-process
//        if (ErrorHandler.Succeeded(hresult))
//        {
//            if (pguidCmdGroup == VSConstants.VSStd2K)
//            {
//                switch ((VSConstants.VSStd2KCmdID)nCmdID)
//                {
//                    case VSConstants.VSStd2KCmdID.TYPECHAR:
//                    case VSConstants.VSStd2KCmdID.BACKSPACE:
//                    case VSConstants.VSStd2KCmdID.DELETE:
//                        this.Filter();
//                        break;
//                }
//            }
//        }
//        #endregion

//        return hresult;
//    }
//    private bool StartSession()
//    {
//        if (this._currentSession != null)
//        {
//            return false;
//        }
//        SnapshotPoint caret = this.TextView.Caret.Position.BufferPosition;
//        ITextSnapshot snapshot = caret.Snapshot;

//        if (this.Broker.IsCompletionActive(this.TextView))
//        {
//            //AsmDudeToolsStatic.Output_INFO(string.Format(AsmDudeToolsStatic.CultureUI, "{0}:StartSession. Recycling an existing auto-complete session", this.ToString()));
//            this._currentSession = this.Broker.GetSessions(this.TextView)[0];
//        }
//        else
//        {
//            //AsmDudeToolsStatic.Output_INFO(string.Format(AsmDudeToolsStatic.CultureUI, "{0}:StartSession. Creating a new auto-complete session", this.ToString()));
//            this._currentSession = this.Broker.CreateCompletionSession(this.TextView, snapshot.CreateTrackingPoint(caret, PointTrackingMode.Positive), true);
//        }
//        this._currentSession.Dismissed += (sender, args) => { this._currentSession = null; };
//        this._currentSession.Start();
//        //AsmDudeToolsStatic.Output_INFO(string.Format(AsmDudeToolsStatic.CultureUI, "{0}:StartSession", this.ToString()));
//        return true;
//    }

//    /// <summary>
//    /// Complete the auto-complete
//    /// </summary>
//    /// <param name="force">force the selection even if it has not been manually selected</param>
//    /// <returns></returns>
//    private bool Complete(bool force)
//    {
//        if (this._currentSession == null)
//        {
//            return false;
//        }
//        if (!this._currentSession.SelectedCompletionSet.SelectionStatus.IsSelected && !force)
//        {
//            this._currentSession.Dismiss();
//            return false;
//        }
//        else
//        {
//            this._currentSession.Commit();
//            return true;
//        }
//    }

//    private bool Cancel()
//    {
//        if (this._currentSession == null)
//        {
//            return false;
//        }
//        this._currentSession.Dismiss();
//        return true;
//    }

//    /// <summary>
//    /// Narrow down the list of options as the user types input
//    /// </summary>
//    private void Filter()
//    {
//        if (this._currentSession == null)
//        {
//            return;
//        }
//        //_currentSession = null;
//        var Options = GeneralOptions.Instance;
//        var shouldProcessChecker = StringPreMatchUtil.GetPreCheckPredicate(Options.PreMatchType, Options.PreCheckRule);
//        var inputMethodDictionaryGroup = _inputMethodDictionaryGroup;
//        string getFilterText(string t)
//        {
//            if (!shouldProcessChecker.Check(t))
//            {
//                return t;
//            }
//            var spellings = inputMethodDictionaryGroup.FindAll(t);
//            var res = Options.EnableMultipleSpellings ? string.Join("/", spellings) : spellings[0];
//            return t + "/" + res;
//        }
//        // this._currentSession.SelectedCompletionSet.SelectBestMatch();
//        //this._currentSession.SelectedCompletionSet.Recalculate();
//        foreach (var item in this._currentSession.CompletionSets)
//        {
//            foreach (var item2 in item.Completions)
//            {
//                item2.DisplayText = getFilterText(item2.DisplayText);
//            }
//        }
//        this._currentSession.Filter();
//    }

//    public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
//    {
//        ThreadHelper.ThrowIfNotOnUIThread();
//        //AsmDudeToolsStatic.Output_INFO(string.Format(AsmDudeToolsStatic.CultureUI, "{0}:QueryStatus", this.ToString()));
//        if (pguidCmdGroup == VSConstants.VSStd2K)
//        {
//            switch ((VSConstants.VSStd2KCmdID)prgCmds[0].cmdID)
//            {
//                case VSConstants.VSStd2KCmdID.AUTOCOMPLETE:
//                case VSConstants.VSStd2KCmdID.COMPLETEWORD:
//                    //AsmDudeToolsStatic.Output_INFO(string.Format(AsmDudeToolsStatic.CultureUI, "{0}:QueryStatus", this.ToString()));
//                    //Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "INFO: {0}:QueryStatus", this.ToString()));
//                    prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_ENABLED | (uint)OLECMDF.OLECMDF_SUPPORTED;
//                    return VSConstants.S_OK;
//            }
//        }
//        return this.NextCommandHandler.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
//    }
//}
