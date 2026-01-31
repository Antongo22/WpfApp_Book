using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp_Book
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_1_Click(object sender, RoutedEventArgs e)
        {
            _1_window _1_Window = new _1_window();
            _1_Window.Show();
        }

        private void Button_2_Click(object sender, RoutedEventArgs e)
        {
            _2_window _2_Window = new _2_window();
            _2_Window.Show();
        }
    }
}