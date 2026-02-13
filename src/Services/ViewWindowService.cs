using Presentation.Wpf.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Minimal.Mvvm.Wpf
{
    /// <summary>
    /// Implementation of <see cref="IViewWindowService"/> that creates windows for view models
    /// by resolving views through the view locator and configuring window properties.
    /// </summary>
    public class ViewWindowService : ViewServiceBase, IViewWindowService
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether newly created windows should have an owner window set.
        /// </summary>
        public bool SetWindowOwner
        {
            get => WindowHelper.GetSetWindowOwner(this);
            set => WindowHelper.SetSetWindowOwner(this, value);
        }

        /// <summary>
        /// Gets or sets the startup location for newly created windows.
        /// </summary>
        public WindowStartupLocation WindowStartupLocation
        {
            get => WindowHelper.GetWindowStartupLocation(this);
            set => WindowHelper.SetWindowStartupLocation(this, value);
        }

        /// <summary>
        /// Gets or sets the style to apply to newly created windows.
        /// </summary>
        public Style? WindowStyle
        {
            get => WindowHelper.GetWindowStyle(this);
            set => WindowHelper.SetWindowStyle(this, value);
        }

        /// <summary>
        /// Gets or sets the resource key for the window style to look up in resources.
        /// </summary>
        public string? WindowStyleKey
        {
            get => WindowHelper.GetWindowStyleKey(this);
            set => WindowHelper.SetWindowStyleKey(this, value);
        }

        /// <summary>
        /// Gets or sets the style selector to choose a window style for newly created windows.
        /// </summary>
        public StyleSelector? WindowStyleSelector
        {
            get => WindowHelper.GetWindowStyleSelector(this);
            set => WindowHelper.SetWindowStyleSelector(this, value);
        }

        /// <summary>
        /// Gets or sets the type of window to create. Uses standard Window type if not specified.
        /// </summary>
        public Type? WindowType
        {
            get => WindowHelper.GetWindowType(this);
            set => WindowHelper.SetWindowType(this, value);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public async ValueTask ShowAsync(string? viewName, object? viewModel, object? parentViewModel, object? parameter,
            CancellationToken cancellationToken = default)
        {
            var window = await CreateWindowAsync(viewName, viewModel, parentViewModel, parameter, cancellationToken);
            window.Show();
        }

        /// <inheritdoc/>
        public async ValueTask<bool?> ShowDialogAsync(string? viewName, object? viewModel, object? parentViewModel, object? parameter,
            CancellationToken cancellationToken = default)
        {
            var window = await CreateWindowAsync(viewName, viewModel, parentViewModel, parameter, cancellationToken);
            return window.ShowDialog();
        }

        private async ValueTask<Window> CreateWindowAsync(string? viewName, object? viewModel, object? parentViewModel, object? parameter,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var view = await CreateViewAsync(viewName, cancellationToken);
            Throw.IfNull(view, "View cannot be null.");

            var window = WindowHelper.CreateWindow(this, view, viewModel);

            var lifetime = new Lifetime()
#if DEBUG
                    .SetDebugInfo()
#endif
                ;
            lifetime.Add(() => window.ClearStyle());//clear window style
            lifetime.AddBracket(
                () => ViewModelHelper.SetDataContextBinding(view, FrameworkElement.DataContextProperty, window),
                () => BindingOperations.ClearBinding(window, FrameworkElement.DataContextProperty));////detach vm
            lifetime.Add(() => ViewModelHelper.DetachContentFromContainer(window));//detach content

            lifetime.AddBracket(
                () => window.Closed += OnWindowClosed,
                () => window.Closed -= OnWindowClosed);

            if (viewModel != null && ViewModelHelper.ViewModelHasTitleProperty(viewModel))
            {
                lifetime.AddBracket(
                () => ViewModelHelper.SetViewTitleBinding(view, Window.TitleProperty, window),
                () => BindingOperations.ClearBinding(window, Window.TitleProperty));
            }

            try
            {
                await ViewModelHelper.InitializeViewAsync(view, viewModel, parentViewModel, parameter, cancellationToken);
            }
            catch
            {
                Disposable.DisposeAndNull(ref lifetime);
                throw;
            }

            return window;

            void OnWindowClosed(object? sender, EventArgs e)
            {
                Disposable.DisposeAndNull(ref lifetime);
            }
        }

        #endregion
    }
}
