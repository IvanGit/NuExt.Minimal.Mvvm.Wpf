using Minimal.Behaviors.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace Minimal.Mvvm.Wpf
{
    /// <summary>
    /// XAML helper to assign a parent ViewModel to an <see cref="IParentedViewModel"/>
    /// when a child view's DataContext (or Content for <see cref="ContentPresenter"/>) becomes available.
    /// One-shot by default; set StickyParentBinding="True" to keep the assignment
    /// in sync on each DataContext change. In sticky mode lifecycle hooks (Loaded/Unloaded)
    /// are used to avoid holding subscriptions while the element is off the visual tree.
    /// </summary>
    public static class ViewModelExtensions
    {
        #region Dependency Properties

        /// <summary>
        /// Gets/sets the parent ViewModel object that will be assigned into
        /// <see cref="IParentedViewModel.ParentViewModel"/> of a child ViewModel.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The helper inspects the associated view to obtain its ViewModel:
        /// for <see cref="ContentPresenter"/> it prefers <c>Content</c> (if available),
        /// otherwise uses <c>DataContext</c>; for other views it uses <c>DataContext</c>.
        /// </para>
        /// <para>
        /// If the ViewModel is not available at the moment of assignment, the helper defers until
        /// the first <c>DataContextChanged</c>. In sticky mode it will track subsequent changes as well.
        /// </para>
        /// <para><strong>Thread‑affinity:</strong> must be used on the UI thread.</para>
        /// <para><strong>Cycle guard:</strong> cyclic parent chains are ignored (no assignment is performed).</para>
        /// </remarks>
        public static readonly DependencyProperty ParentViewModelProperty = DependencyProperty.RegisterAttached(
            "ParentViewModel", typeof(object), typeof(ViewModelExtensions), 
            new PropertyMetadata(defaultValue: null, (d, e) => OnParentViewModelChanged(d, e.NewValue)));

        /// <summary>
        /// Enables sticky behavior: the parent is reassigned on every <c>DataContext</c> change.
        /// </summary>
        /// <remarks>
        /// When enabled, the helper attaches on <c>Loaded</c> and detaches on <c>Unloaded</c>
        /// to avoid holding subscriptions while off the visual tree.
        /// </remarks>

        public static readonly DependencyProperty StickyParentBindingProperty =  DependencyProperty.RegisterAttached(
                "StickyParentBinding",  typeof(bool), typeof(ViewModelExtensions),
                new PropertyMetadata(defaultValue: false, (d, e) =>
                {
                    var parentViewModel = GetParentViewModel(d);
                    ApplyOrHook(d, parentViewModel, (bool)e.NewValue);
                }));

        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        [AttachedPropertyBrowsableForType(typeof(FrameworkContentElement))]
        public static void SetParentViewModel(DependencyObject obj, object? value)
            => obj.SetValue(ParentViewModelProperty, value);

        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        [AttachedPropertyBrowsableForType(typeof(FrameworkContentElement))]
        public static object? GetParentViewModel(DependencyObject obj)
            => obj.GetValue(ParentViewModelProperty);

        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        [AttachedPropertyBrowsableForType(typeof(FrameworkContentElement))]
        public static void SetStickyParentBinding(DependencyObject obj, bool value)
            => obj.SetValue(StickyParentBindingProperty, value);

        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        [AttachedPropertyBrowsableForType(typeof(FrameworkContentElement))]
        public static bool GetStickyParentBinding(DependencyObject obj)
            => (bool)obj.GetValue(StickyParentBindingProperty);

        #endregion

        #region Event Handlers

        private static void OnParentViewModelChanged(DependencyObject view, object? parentViewModel)
        {
            ApplyOrHook(view, parentViewModel, GetStickyParentBinding(view));
        }

        private static void ApplyOrHook(DependencyObject view, object? parentViewModel, bool sticky)
        {
            DetachDataContextChanged(view);
            DetachLifecycle(view);

            var viewModel = GetViewModelFromView(view);
            if (TryApply(viewModel, parentViewModel, out bool needsDefer))
            {
                if (sticky)
                {
                    AttachLifecycle(view);
                    AttachDataContextChanged(view);
                }
                return;
            }

            // No VM yet: with sticky -> attach lifecycle + DC; without sticky -> one-shot DC.
            if (sticky)
            {
                AttachLifecycle(view);
                AttachDataContextChanged(view);
            }
            else if (needsDefer)
            {
                AttachDataContextChanged(view);
            }
        }

        private static void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var view = (DependencyObject)sender;
            var parentViewModel = GetParentViewModel(view);
            var sticky = GetStickyParentBinding(view);
            var viewModel = GetViewModelFromView(view);

            if (!sticky)
            {
                DetachDataContextChanged(view);
            }

            TryApply(viewModel, parentViewModel, out _);
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            var view = (DependencyObject)sender;
            if (GetStickyParentBinding(view))
            {
                AttachDataContextChanged(view);
                // Optional eager apply on load (covers late XAML scenarios)
                TryApply(GetViewModelFromView(view), GetParentViewModel(view), out _);
            }
        }

        private static void OnUnloaded(object sender, RoutedEventArgs e)
        {
            var view = (DependencyObject)sender;
            if (GetStickyParentBinding(view))
            {
                DetachDataContextChanged(view);
            }
        }

        #endregion

        #region Methods (attach/detach)

        private static void AttachLifecycle(DependencyObject view)
        {
            switch (view)
            {
                case FrameworkElement fe:
                    fe.Loaded -= OnLoaded;
                    fe.Unloaded -= OnUnloaded;
                    fe.Loaded += OnLoaded;
                    fe.Unloaded += OnUnloaded;
                    break;

                case FrameworkContentElement fce:
                    fce.Loaded -= OnLoaded;
                    fce.Unloaded -= OnUnloaded;
                    fce.Loaded += OnLoaded;
                    fce.Unloaded += OnUnloaded;
                    break;
            }
        }

        private static void DetachLifecycle(DependencyObject view)
        {
            switch (view)
            {
                case FrameworkElement fe:
                    fe.Loaded -= OnLoaded;
                    fe.Unloaded -= OnUnloaded;
                    break;

                case FrameworkContentElement fce:
                    fce.Loaded -= OnLoaded;
                    fce.Unloaded -= OnUnloaded;
                    break;
            }
        }

        private static void AttachDataContextChanged(DependencyObject view)
        {
            switch (view)
            {
                case ContentPresenter cp:
                    cp.DataContextChanged -= OnDataContextChanged;
                    cp.DataContextChanged += OnDataContextChanged;
                    break;

                case FrameworkElement fe:
                    fe.DataContextChanged -= OnDataContextChanged;
                    fe.DataContextChanged += OnDataContextChanged;
                    break;

                case FrameworkContentElement fce:
                    fce.DataContextChanged -= OnDataContextChanged;
                    fce.DataContextChanged += OnDataContextChanged;
                    break;
            }
        }

        private static void DetachDataContextChanged(DependencyObject view)
        {
            switch (view)
            {
                case ContentPresenter cp:
                    cp.DataContextChanged -= OnDataContextChanged;
                    break;

                case FrameworkElement fe:
                    fe.DataContextChanged -= OnDataContextChanged;
                    break;

                case FrameworkContentElement fce:
                    fce.DataContextChanged -= OnDataContextChanged;
                    break;
            }
        }

        #endregion

        #region Methods (core logic)

        private static bool TryApply(object? viewModel, object? parentViewModel, out bool lazyInit)
        {
            if (viewModel is IParentedViewModel child)
            {
                // Guard against cyclic assignment
                var current = parentViewModel as IParentedViewModel;
                while (current != null)
                {
                    if (ReferenceEquals(current, child))
                    {
                        lazyInit = false;
                        return false;
                    }
                    current = current.ParentViewModel as IParentedViewModel;
                }

                if (!ReferenceEquals(child.ParentViewModel, parentViewModel))
                {
                    child.ParentViewModel = parentViewModel;
                }

                lazyInit = false;
                return true;
            }

            lazyInit = viewModel is null;
            return false;
        }

        private static object? GetViewModelFromView(DependencyObject view) =>
            Interaction.GetDataContext(view);


        #endregion
    }
}
