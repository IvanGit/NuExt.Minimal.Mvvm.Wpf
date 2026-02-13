using System.ComponentModel;
using System.Threading;

namespace Minimal.Mvvm.Wpf
{
    /// <summary>
    /// Base ViewModel interface for UI controls with thread synchronization support.
    /// </summary>
    public interface IControlViewModel : IParentedViewModel, IParameterizedViewModel, IAsyncDisposableNotifiable, ISynchronizeInvoker
    {
    }
}
