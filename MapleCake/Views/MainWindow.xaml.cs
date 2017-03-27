using System.Windows;
using MapleCake.ViewModels;

namespace MapleCake.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnContextMenuClosing(object sender, RoutedEventArgs e)
        {
            ((MainWindowViewModel) DataContext)?.Config.RaisePropertyChangedEvent("ContextItems");
        }
    }
}