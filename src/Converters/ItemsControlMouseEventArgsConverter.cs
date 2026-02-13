using System.Diagnostics;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;

namespace Minimal.Mvvm.Wpf
{
    public class ItemsControlMouseEventArgsConverter : ValueConverterBase<MouseEventArgs, object?, Control>
    {
        public ItemsControlMouseEventArgsConverter()
        {

        }
        protected override object? ConvertTo(MouseEventArgs? args, Control? sender, CultureInfo? culture)
        {
            return sender?.DataContext;
        }
    }
}
