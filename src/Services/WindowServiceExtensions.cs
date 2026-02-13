using System.Windows;

namespace Minimal.Mvvm.Wpf
{
    public static class WindowServiceExtensions
    {
        extension(IWindowService windowService)
        {
            /// <summary>
            /// Gets the Window associated with this service.
            /// </summary>
            public Window? Window => windowService is WindowService service ? service.Window : null;
        }
    }
}
