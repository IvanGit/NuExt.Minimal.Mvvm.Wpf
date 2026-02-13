using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Minimal.Mvvm.Wpf
{
    /// <summary>
    /// Defines the contract for a window ViewModel.
    /// </summary>
    public interface IWindowViewModel : IControlViewModel
    {
        /// <summary>
        /// Gets the cancellation token for window lifecycle.
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Gets or sets the window title.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets the command to close the window.
        /// </summary>
        ICommand? CloseCommand { get; }

        /// <summary>
        /// Determines whether the window can be closed asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The result indicates whether the window can be closed.
        /// </returns>
        ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously closes the window and disposes the ViewModel if closure is not canceled. 
        /// A disposed ViewModel should not be reused.
        /// </summary>
        /// <param name="force">
        /// <see langword="true"/> to close immediately; 
        /// <see langword="false"/> to allow cancellation via <see cref="CanCloseAsync"/>.
        /// </param>
        ValueTask CloseAsync(bool force = true);

        /// <summary>
        /// Hides the window.
        /// </summary>
        void Hide();

        /// <summary>
        /// Shows the window.
        /// </summary>
        void Show();

    }
}
