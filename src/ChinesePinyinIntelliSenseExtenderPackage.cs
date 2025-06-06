using System.Runtime.InteropServices;
using System.Windows;

using ChinesePinyinIntelliSenseExtender.Options;
using ChinesePinyinIntelliSenseExtender.Util;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Task = System.Threading.Tasks.Task;

namespace ChinesePinyinIntelliSenseExtender;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[Guid(PackageGuidString)]
[ProvideOptionPage(typeof(OptionPages.General), PackageName, "常规", 0, 0, true)]
[ProvideOptionPage(typeof(OptionPages.DictionaryManage), PackageName, "字典", 0, 0, true)]
[ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideAutoLoad(UIContextGuids80.SolutionHasMultipleProjects, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideAutoLoad(UIContextGuids80.SolutionHasSingleProject, PackageAutoLoadFlags.BackgroundLoad)]
public sealed class ChinesePinyinIntelliSenseExtenderPackage : AsyncPackage
{
    #region Public 字段

    /// <summary>
    /// ChinesePinyinIntelliSenseExtenderPackage GUID string.
    /// </summary>
    public const string PackageGuidString = "cd4393db-d533-4077-93da-9fdad98ddacf";

    public const string PackageName = "IntelliSense拼音补全";

    #endregion Public 字段

    #region Package Members

    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await base.InitializeAsync(cancellationToken, progress);

        var options = await DictionaryManageOptions.GetLiveInstanceAsync(cancellationToken);

        _ = InputMethodDictionaryGroupProvider.LoadFromOptionsAsync(options, cancellationToken).ContinueWith(task =>
        {
            if (task.Exception is not null)
            {
                var exception = task.Exception.InnerException?.ToString();
                cancellationToken.ThrowIfCancellationRequested();
                MessageBox.Show($"Load options failed with \"{exception}\"", PackageName);
            }
        }, cancellationToken, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current);
    }

    #endregion Package Members
}
