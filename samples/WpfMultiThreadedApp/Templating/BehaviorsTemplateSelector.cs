using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using WpfMultiThreadedApp.ViewModels;

namespace WpfMultiThreadedApp.Templating
{
    public sealed class BehaviorsTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? WindowManagerDataTemplate { get; set; }

        public DataTemplate? ViewFactoryDataTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            Debug.Assert(item is MainWindowViewModel);
            if (item is MainWindowViewModel viewModel)
            {
                return viewModel.UseWindowManager ? WindowManagerDataTemplate : ViewFactoryDataTemplate;
            }
            return base.SelectTemplate(item, container);
        }
    }
}
