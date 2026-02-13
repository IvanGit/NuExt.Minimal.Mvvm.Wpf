using System.Windows;
using System.Windows.Controls;

namespace Minimal.Mvvm.Wpf.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)] // WPF requires STA
    public sealed class ViewModelExtensionsTests
    {
        // ---------- Helpers ----------

        internal sealed class TestChildViewModel : IParentedViewModel
        {
            public int AssignCount { get; private set; }

            public object? ParentViewModel
            {
                get;
                set
                {
                    if (!ReferenceEquals(field, value))
                    {
                        field = value;
                        AssignCount++;
                    }
                }
            }
        }

        internal sealed class TestParentViewModel : IParentedViewModel
        {
            public object? ParentViewModel { get; set; }
        }

        private static void RaiseLoaded(FrameworkElement fe)
            => fe.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

        private static void RaiseUnloaded(FrameworkElement fe)
            => fe.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));

        // ---------- One-shot (immediate DC) ----------

        [Test]
        public void OneShot_Assigns_WhenDataContextIsPresent_And_DoesNotReassignOnLaterChanges()
        {
            var parent = new TestParentViewModel();
            var child1 = new TestChildViewModel();
            var child2 = new TestChildViewModel();

            var view = new Grid { DataContext = child1 };

            // one-shot (Sticky=false by default)
            ViewModelExtensions.SetParentViewModel(view, parent);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(child1.ParentViewModel, Is.SameAs(parent));
                Assert.That(child1.AssignCount, Is.EqualTo(1));
            }

            // Change DataContext to another VM -> one-shot shouldn't assign anymore
            view.DataContext = child2;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(child2.ParentViewModel, Is.Null);
                Assert.That(child2.AssignCount, Is.Zero);
            }

            // Re-assigning the same parent explicitly does not count as another assignment
            ViewModelExtensions.SetParentViewModel(view, parent);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(child2.ParentViewModel, Is.Null);
                Assert.That(child1.AssignCount, Is.EqualTo(1));
            }
        }

        // ---------- One-shot (deferred until DC appears) ----------

        [Test]
        public void OneShot_Assigns_WhenDataContextBecomesAvailable_Once()
        {
            var parent = new TestParentViewModel();
            var child1 = new TestChildViewModel();
            var child2 = new TestChildViewModel();

            var view = new Grid(); // no DataContext yet

            ViewModelExtensions.SetParentViewModel(view, parent);
            // No assignment yet
            Assert.That(child1.ParentViewModel, Is.Null);

            // First DC appears -> assignment happens once
            view.DataContext = child1;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(child1.ParentViewModel, Is.SameAs(parent));
                Assert.That(child1.AssignCount, Is.EqualTo(1));
            }

            // Next DC change in one-shot mode -> should not assign
            view.DataContext = child2;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(child2.ParentViewModel, Is.Null);
                Assert.That(child2.AssignCount, Is.Zero);
            }
        }

        // ---------- Sticky: reassign on every DC change + lifecycle ----------

        [Test]
        public void Sticky_Reassigns_OnEachChange_And_Respects_Loaded_Unloaded()
        {
            var parent = new TestParentViewModel();
            var c1 = new TestChildViewModel();
            var c2 = new TestChildViewModel();
            var c3 = new TestChildViewModel();

            var view = new Grid();

            // Enable sticky first (order doesn't matter for our impl)
            ViewModelExtensions.SetStickyParentBinding(view, true);
            ViewModelExtensions.SetParentViewModel(view, parent);

            // Simulate element entering visual tree
            RaiseLoaded(view);

            // First DC
            view.DataContext = c1;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(c1.ParentViewModel, Is.SameAs(parent));
                Assert.That(c1.AssignCount, Is.EqualTo(1));
            }

            // Change DC -> sticky must re-assign to new VM
            view.DataContext = c2;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(c2.ParentViewModel, Is.SameAs(parent));
                Assert.That(c2.AssignCount, Is.EqualTo(1));
            }

            // Simulate leaving visual tree -> detach DC handler
            RaiseUnloaded(view);

            // Change DC while unloaded -> must NOT assign
            view.DataContext = c3;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(c3.ParentViewModel, Is.Null);
                Assert.That(c3.AssignCount, Is.Zero);
            }

            // Loaded again -> subscription restored; next change should assign
            RaiseLoaded(view);
            var c4 = new TestChildViewModel();
            view.DataContext = c4;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(c4.ParentViewModel, Is.SameAs(parent));
                Assert.That(c4.AssignCount, Is.EqualTo(1));
            }
        }

        // ---------- ContentPresenter: Content preferred over DataContext ----------

        [Test]
        public void ContentPresenter_Prefers_Content_Over_DataContext_ForInitialApply()
        {
            var parent = new TestParentViewModel();
            var contentVm = new TestChildViewModel();
            var dcVm = new TestChildViewModel();

            var cp = new ContentPresenter
            {
                // Both are set; implementation should prefer Content
                Content = contentVm,
                DataContext = dcVm
            };

            ViewModelExtensions.SetParentViewModel(cp, parent);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(contentVm.ParentViewModel, Is.SameAs(parent));
                Assert.That(contentVm.AssignCount, Is.EqualTo(1));
                Assert.That(dcVm.ParentViewModel, Is.Null);
                Assert.That(dcVm.AssignCount, Is.Zero);
            }
        }

        // ---------- Cycle guard ----------

        [Test]
        public void Cycle_IsIgnored_And_DoesNotAssign()
        {
            var parent = new TestParentViewModel();
            var child = new TestChildViewModel();

            // Create cycle: parent -> child
            parent.ParentViewModel = child;

            var view = new Grid { DataContext = child };
            ViewModelExtensions.SetParentViewModel(view, parent);

            using (Assert.EnterMultipleScope())
            {
                // Guard must detect cycle and skip assignment
                Assert.That(child.ParentViewModel, Is.Null);
                Assert.That(child.AssignCount, Is.Zero);
            }
        }

        // ---------- Idempotent set: same parent doesn't increment ----------

        [Test]
        public void Idempotent_SameParent_DoesNotIncrementAssignCount()
        {
            var parent = new TestParentViewModel();
            var child = new TestChildViewModel();
            var view = new Grid { DataContext = child };

            ViewModelExtensions.SetParentViewModel(view, parent);
            Assert.That(child.AssignCount, Is.EqualTo(1));

            // Same parent again -> no increment
            ViewModelExtensions.SetParentViewModel(view, parent);
            Assert.That(child.AssignCount, Is.EqualTo(1));
        }
    }
}