using System.Windows.Controls;

namespace Presentation.Wpf.Controls
{
    public static class ControlExtensions
    {
        public static void ClearStyle(this Control? control)
        {
            if (control is null)
            {
                return;
            }

            control.Template = null;
            control.Style = null;
        }
    }
}
