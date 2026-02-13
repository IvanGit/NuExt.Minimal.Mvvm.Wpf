using System.Windows;

namespace WpfMultiThreadedApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(object dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
        }
    }
}