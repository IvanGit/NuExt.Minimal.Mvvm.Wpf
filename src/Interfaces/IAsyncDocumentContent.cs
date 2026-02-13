using System.Threading;
using System.Threading.Tasks;

namespace Minimal.Mvvm.Wpf
{
    /// <summary>
    /// Defines an interface for asynchronous document content that can represent a view model.
    /// </summary>
    public interface IAsyncDocumentContent
    {
        /// <summary>
        /// Gets the title of the document content.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Determines whether the document content can be closed asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The result indicates whether the document can be closed.
        /// </returns>
        ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken);
    }
}
